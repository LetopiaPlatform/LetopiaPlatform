using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.Auth.Request;
using LetopiaPlatform.Core.DTOs.Auth.Response;
using LetopiaPlatform.Core.DTOs.UserRefershToken.Request;
using LetopiaPlatform.Core.Entities.Identity;
using LetopiaPlatform.Core.Interfaces;
using LetopiaPlatform.Core.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace LetopiaPlatform.Infrastructure.Identity;

public class AuthService : IAuthService
{
    private const string GoogleProvider = "Google";

    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IGoogleTokenValidator _googleTokenValidator;

    public AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IJwtTokenService jwtTokenService,
        IGoogleTokenValidator googleTokenValidator)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
        _googleTokenValidator = googleTokenValidator;
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
        {
            var errors = identityResult.Errors.Select(e => e.Description).ToList();
            return Result<AuthResponse>.Failure(errors, 400);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, "Learner");
        if (!roleResult.Succeeded)
        {
            return Result<AuthResponse>.Failure("Failed to assign default role.", 500);
        }

        var authResponse = await _jwtTokenService.GetJWTTokenAsync(user, cancellationToken);

        return Result<AuthResponse>.Success(authResponse, 201);
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Result<AuthResponse>.Failure("Invalid email or password.", 401);

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!signInResult.Succeeded)
            return Result<AuthResponse>.Failure("Invalid email or password.", 401);

        var authResponse = await _jwtTokenService.GetJWTTokenAsync(user, cancellationToken);

        return Result<AuthResponse>.Success(authResponse);
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default)
    {
        return await _jwtTokenService.RefreshTokenAsync(request.AccessToken, request.RefreshToken, cancellationToken);
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
            var authResponse = await _jwtTokenService.GetJWTTokenAsync(user, cancellationToken);
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

            // Check condition: Only update avatar if currently empty
            if (string.IsNullOrEmpty(user.AvatarUrl))
            {
                user.AvatarUrl = googleUserInfo.PictureUrl;
            }

            user.UpdatedAt = DateTime.UtcNow;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return Result<AuthResponse>.Failure("Failed to update user info.", 500);

            var authResponse = await _jwtTokenService.GetJWTTokenAsync(user, cancellationToken);
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

        var roleRes = await _userManager.AddToRoleAsync(user, "Learner");
        if (!roleRes.Succeeded)
            return Result<AuthResponse>.Failure("Failed to assign default role.", 500);

        var finalAuthResponse = await _jwtTokenService.GetJWTTokenAsync(user, cancellationToken);
        return Result<AuthResponse>.Success(finalAuthResponse, 201);
    }
}
