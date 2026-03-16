using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.Auth.Request;
using LetopiaPlatform.Core.DTOs.Auth.Response;
using LetopiaPlatform.Core.DTOs.UserRefershToken.Request;
using LetopiaPlatform.Core.Entities.Identity;
using LetopiaPlatform.Core.Interfaces;
using LetopiaPlatform.Core.Interfaces.Repositories;
using LetopiaPlatform.Core.Services.Interfaces;
using LetopiaPlatform.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LetopiaPlatform.Infrastructure.Identity;

public class AuthService : IAuthService
{
    private const string GoogleProvider = "Google";

    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IGoogleTokenValidator _googleTokenValidator;
    private readonly IUnitOfWork<ApplicationDbContext> _unitOfWork;
    private readonly IUserRefreshTokenRepository _userRefreshTokenRepository;

    public AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IJwtTokenService jwtTokenService,
        IGoogleTokenValidator googleTokenValidator,
        IUnitOfWork<ApplicationDbContext> unitOfWork,
        IUserRefreshTokenRepository userRefreshTokenRepository)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
        _googleTokenValidator = googleTokenValidator;
        _unitOfWork = unitOfWork;
        _userRefreshTokenRepository = userRefreshTokenRepository;
    }

    public async Task<Result<AuthResponse>> SignUpAsync(SignUpRequest request, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            return Result<AuthResponse>.Failure("User with this email already exists.", 409);

        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var identityResult = await _userManager.CreateAsync(user, request.Password);
        if (!identityResult.Succeeded)
            return Result<AuthResponse>.Failure(identityResult.Errors.Select(e => e.Description).ToList(), 400);

        await _userManager.AddToRoleAsync(user, "Learner");

        var authResponse = await CreateFullAuthResponseAsync(user, cancellationToken);
        return Result<AuthResponse>.Success(authResponse, 201);
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null) return Result<AuthResponse>.Failure("Invalid email or password.", 401);

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!signInResult.Succeeded) return Result<AuthResponse>.Failure("Invalid email or password.", 401);

        var authResponse = await CreateFullAuthResponseAsync(user, cancellationToken);
        return Result<AuthResponse>.Success(authResponse);
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default)
    {
        var principal = _jwtTokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null) return Result<AuthResponse>.Failure("Invalid access token", 400);

        var userIdClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var jti = principal.FindFirstValue(JwtRegisteredClaimNames.Jti);

        if (!Guid.TryParse(userIdClaim, out Guid userId))
            return Result<AuthResponse>.Failure("Invalid token claims", 400);

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return Result<AuthResponse>.Failure("User not found", 404);

        var refreshTokenHash = ComputeSha256Hash(request.RefreshToken);
        var storedToken = await _userRefreshTokenRepository.GetTableAsTracking()
            .FirstOrDefaultAsync(x => x.RefreshTokenHash == refreshTokenHash && x.UserId == userId, cancellationToken);

        if (storedToken == null || storedToken.IsUsed || storedToken.IsRevoked || storedToken.JwtId != jti || storedToken.ExpiryDate < DateTime.UtcNow)
            return Result<AuthResponse>.Failure("Invalid, expired or reused refresh token", 401);

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            storedToken.IsUsed = true;
            await _userRefreshTokenRepository.UpdateAsync(storedToken);

            var authResponse = await CreateFullAuthResponseAsync(user, cancellationToken);

            await _unitOfWork.CommitAsync();
            return Result<AuthResponse>.Success(authResponse);
        }
        catch (DbUpdateConcurrencyException)
        {
            await _unitOfWork.RollbackAsync();
            return Result<AuthResponse>.Failure("Security Alert: Token is being used simultaneously.", 409);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<Result<AuthResponse>> GoogleLoginAsync(GoogleLoginRequest request, CancellationToken cancellationToken = default)
    {
        var googleUserInfo = await _googleTokenValidator.ValidateAsync(request.AccessToken);
        if (googleUserInfo == null)
        {
            return Result<AuthResponse>.Failure("Invalid Google token.", 401);
        }

        // 1. Check if user already has this Google login linked
        var user = await _userManager.FindByLoginAsync(GoogleProvider, googleUserInfo.GoogleId);
        if (user != null)
        {
            var authResponse = await CreateFullAuthResponseAsync(user, cancellationToken);
            return Result<AuthResponse>.Success(authResponse);
        }

        // 2. Check if email exists but not linked to Google
        user = await _userManager.FindByEmailAsync(googleUserInfo.Email);
        if (user != null)
        {
            var loginResult = await _userManager.AddLoginAsync(user, new UserLoginInfo(GoogleProvider, googleUserInfo.GoogleId, GoogleProvider));


            if (!loginResult.Succeeded)
                return Result<AuthResponse>.Failure("Failed to link Google account.", 500);

            user.EmailConfirmed = true;
            user.EmailVerified = true;


            if (string.IsNullOrEmpty(user.AvatarUrl))
            {
                user.AvatarUrl = googleUserInfo.PictureUrl;
            }

            user.UpdatedAt = DateTime.UtcNow;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return Result<AuthResponse>.Failure("Failed to update user info.", 500);

            var authResponse = await CreateFullAuthResponseAsync(user, cancellationToken);
            return Result<AuthResponse>.Success(authResponse);
        }

        // 3. New User Flow
        user = new User
        {
            UserName = googleUserInfo.Email,
            Email = googleUserInfo.Email,
            FullName = googleUserInfo.Name,
            EmailConfirmed = true,
            EmailVerified = true,
            AvatarUrl = googleUserInfo.PictureUrl,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user);
        if (!createResult.Succeeded)
        {
            var errors = createResult.Errors.Select(e => e.Description).ToList();
            return Result<AuthResponse>.Failure(errors, 400);
        }


        var addLoginRes = await _userManager.AddLoginAsync(user, new UserLoginInfo(GoogleProvider, googleUserInfo.GoogleId, GoogleProvider));
        if (!addLoginRes.Succeeded)
            return Result<AuthResponse>.Failure("Failed to add Google login info.", 500);

        var addRoleRes = await _userManager.AddToRoleAsync(user, "Learner");
        if (!addRoleRes.Succeeded)
            return Result<AuthResponse>.Failure("Failed to assign default role.", 500);

        var finalAuthResponse = await CreateFullAuthResponseAsync(user, cancellationToken);
        return Result<AuthResponse>.Success(finalAuthResponse, 201);
    }
    // --- Private Helpers ---

    private async Task<AuthResponse> CreateFullAuthResponseAsync(User user, CancellationToken ct)
    {
        var accessToken = _jwtTokenService.GenerateJwtToken(user);
        var refreshPlain = _jwtTokenService.GenerateRefreshToken();

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(accessToken);
        var jti = jwtToken.Id;
        var expiresAt = jwtToken.ValidTo;

        await _userRefreshTokenRepository.DeleteExpiredTokensAsync(user.Id, ct);

        var userRefreshToken = new UserRefreshToken
        {
            UserId = user.Id,
            JwtId = jti,
            RefreshTokenHash = ComputeSha256Hash(refreshPlain),
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            AddedTime = DateTime.UtcNow
        };

        await _userRefreshTokenRepository.AddAsync(userRefreshToken);
        await _unitOfWork.SaveChangesAsync(ct);

        return new AuthResponse(
            JwtToken: new TokenResult(accessToken, expiresAt),
            RefreshToken: refreshPlain,
            User: new UserDto(user.Id.ToString(), user.Email!, user.FullName!, user.Role ?? "Learner", user.AvatarUrl)
        );
    }

    private static string ComputeSha256Hash(string rawData)
    {
        var bytes = Encoding.UTF8.GetBytes(rawData);
        return Convert.ToBase64String(SHA256.HashData(bytes));
    }
}
