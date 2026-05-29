using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using LetopiaPlatform.Core.AppSettings;
using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.Auth.Request;
using LetopiaPlatform.Core.DTOs.Auth.Response;
using LetopiaPlatform.Core.DTOs.Email;
using LetopiaPlatform.Core.DTOs.UserRefershToken.Request;
using LetopiaPlatform.Core.Entities.Identity;
using LetopiaPlatform.Core.Enums;
using LetopiaPlatform.Core.Interfaces;
using LetopiaPlatform.Core.Interfaces.Repositories;
using LetopiaPlatform.Core.Services.Interfaces;
using LetopiaPlatform.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
    private readonly IEmailService _emailService;
    private readonly string _assetsBaseUrl;
    private readonly string _frontendBaseUrl;

    public AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IJwtTokenService jwtTokenService,
        IGoogleTokenValidator googleTokenValidator,
        IUnitOfWork<ApplicationDbContext> unitOfWork,
        IUserRefreshTokenRepository userRefreshTokenRepository,
        IEmailService emailService,
        IOptions<SmtpSettings> smtpSettings)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
        _googleTokenValidator = googleTokenValidator;
        _unitOfWork = unitOfWork;
        _userRefreshTokenRepository = userRefreshTokenRepository;
        _emailService = emailService;
        _assetsBaseUrl = smtpSettings.Value.EmailAssetsBaseUrl.TrimEnd('/');
        _frontendBaseUrl = smtpSettings.Value.FrontendBaseUrl.TrimEnd('/');
    }

    public async Task<Result> SignUpAsync(SignUpRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            return Result.Failure("User with this email already exists.", 409);

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
            return Result.Failure(errors, 400);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, "Learner");
        if (!roleResult.Succeeded)
            return Result.Failure("Failed to assign default role.", 500);

        await SendCodeToUserAsync(user, OtpPurpose.EmailVerification);
        SendWelcomeEmail(user);

        return Result.Success(201);
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Result<AuthResponse>.Failure("Invalid email or password.", 401);

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!signInResult.Succeeded)
            return Result<AuthResponse>.Failure("Invalid email or password.", 401);

        // if (!user.EmailVerified)
        //     return Result<AuthResponse>.Failure("Email not verified. Please verify your email before logging in.", 403);

        var authResponse = await CreateFullAuthResponseAsync(user, cancellationToken);
        return Result<AuthResponse>.Success(authResponse);
    }

    public async Task<Result<AuthResponse>> GoogleLoginAsync(GoogleLoginRequest request, CancellationToken cancellationToken = default)
    {
        var googleUserInfo = await _googleTokenValidator.ValidateAsync(request.AccessToken);
        if (googleUserInfo == null)
            return Result<AuthResponse>.Failure("Invalid Google token.", 401);

        var user = await _userManager.FindByLoginAsync(GoogleProvider, googleUserInfo.GoogleId);
        if (user != null)
            return Result<AuthResponse>.Success(await CreateFullAuthResponseAsync(user, cancellationToken));

        user = await _userManager.FindByEmailAsync(googleUserInfo.Email);
        if (user != null)
        {
            var loginResult = await _userManager.AddLoginAsync(user,
                new UserLoginInfo(GoogleProvider, googleUserInfo.GoogleId, GoogleProvider));

            if (!loginResult.Succeeded)
                return Result<AuthResponse>.Failure("Failed to link Google account.", 500);

            user.EmailConfirmed = true;
            user.EmailVerified = true;

            if (string.IsNullOrEmpty(user.AvatarUrl))
                user.AvatarUrl = googleUserInfo.PictureUrl;

            user.UpdatedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            return Result<AuthResponse>.Success(await CreateFullAuthResponseAsync(user, cancellationToken));
        }

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

        await _userManager.AddLoginAsync(user,
            new UserLoginInfo(GoogleProvider, googleUserInfo.GoogleId, GoogleProvider));

        await _userManager.AddToRoleAsync(user, "Learner");

        var authResponse = await CreateFullAuthResponseAsync(user, cancellationToken);
        return Result<AuthResponse>.Success(authResponse, 201);
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default)
    {
        var principal = _jwtTokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null)
            return Result<AuthResponse>.Failure("Invalid access token", 400);

        var userIdClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var jti = principal.FindFirstValue(JwtRegisteredClaimNames.Jti);

        if (!Guid.TryParse(userIdClaim, out Guid userId))
            return Result<AuthResponse>.Failure("Invalid token claims", 400);

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return Result<AuthResponse>.Failure("User not found", 404);

        var refreshTokenHash = ComputeSha256Hash(request.RefreshToken);
        var storedToken = await _userRefreshTokenRepository.GetTableAsTracking()
            .FirstOrDefaultAsync(x => x.RefreshTokenHash == refreshTokenHash && x.UserId == userId, cancellationToken);

        if (storedToken == null || storedToken.IsUsed || storedToken.IsRevoked ||
            storedToken.JwtId != jti || storedToken.ExpiryDate < DateTime.UtcNow)
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
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<Result> SendVerificationCodeAsync(SendCodeRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Result.Success();

        await SendCodeToUserAsync(user, request.Purpose);
        return Result.Success();
    }

    public async Task<Result<AuthResponse>> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Result<AuthResponse>.Failure("Invalid email or verification code.", 400);

        var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider, request.Code);
        if (!isValid)
            return Result<AuthResponse>.Failure("Invalid or expired code.", 400);

        user.EmailVerified = true;
        user.EmailConfirmed = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        SendOnboardingEmail(user);

        var authResponse = await CreateFullAuthResponseAsync(user, cancellationToken);
        return Result<AuthResponse>.Success(authResponse);
    }

    public async Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Result.Success();

        await SendCodeToUserAsync(user, OtpPurpose.PasswordReset);
        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Result.Failure("Invalid email or verification code.", 400);

        var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider, request.Code);
        if (!isValid)
            return Result.Failure("Invalid or expired code.", 400);

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, request.NewPassword);

        if (!resetResult.Succeeded)
        {
            var errors = resetResult.Errors.Select(e => e.Description).ToList();
            return Result.Failure(errors, 400);
        }

        return Result.Success();
    }

    #region Private helpers

    private async Task<AuthResponse> CreateFullAuthResponseAsync(User user, CancellationToken ct)
    {
        var accessToken = _jwtTokenService.GenerateJwtToken(user);
        var refreshPlain = _jwtTokenService.GenerateRefreshToken();

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(accessToken);

        await _userRefreshTokenRepository.DeleteExpiredTokensAsync(user.Id, ct);

        await _userRefreshTokenRepository.AddAsync(new UserRefreshToken
        {
            UserId = user.Id,
            JwtId = jwtToken.Id,
            RefreshTokenHash = ComputeSha256Hash(refreshPlain),
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            AddedTime = DateTime.UtcNow
        });

        await _unitOfWork.SaveChangesAsync(ct);

        return new AuthResponse(
            new TokenResult(accessToken, jwtToken.ValidTo),
            refreshPlain,
            new UserDto(user.Id.ToString(), user.Email!, user.FullName!, user.Role ?? "Learner", user.AvatarUrl)
        );
    }

    private async Task SendCodeToUserAsync(User user, OtpPurpose purpose)
    {
        var code = await _userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider);
        var userName = user.FullName ?? EmailTemplates.DefaultUserName;

        var (title, subject, body, afterCodeBody, illustration) = purpose switch
        {
            OtpPurpose.EmailVerification => (
                EmailTemplates.VerifyTitle,
                EmailTemplates.VerifySubject,
                EmailTemplates.VerifyBody,
                EmailTemplates.VerifyAfterCodeBody,
                EmailTemplates.VerifyIllustration
            ),
            OtpPurpose.PasswordReset => (
                EmailTemplates.ResetTitle,
                EmailTemplates.ResetSubject,
                EmailTemplates.ResetBody,
                EmailTemplates.ResetAfterCodeBody,
                EmailTemplates.ResetPasswordIllustration
            ),
            _ => throw new ArgumentException("Invalid verification purpose.")
        };

        _emailService.Enqueue(new EmailMessage(
            To: user.Email!,
            Subject: subject,
            Title: title,
            Body: body,
            UserName: userName,
            Code: code,
            AfterCodeBody: afterCodeBody,
            IllustrationUrl: $"{_assetsBaseUrl}/{illustration}"
        ));
    }

    private void SendWelcomeEmail(User user)
    {
        var userName = user.FullName ?? EmailTemplates.DefaultUserName;

        _emailService.Enqueue(new EmailMessage(
            To: user.Email!,
            Subject: EmailTemplates.WelcomeSubject,
            Title: EmailTemplates.WelcomeTitle,
            Body: EmailTemplates.WelcomeBody,
            UserName: userName,
            IllustrationUrl: $"{_assetsBaseUrl}/{EmailTemplates.WelcomeIllustration}"
        ));
    }

    private void SendOnboardingEmail(User user)
    {
        var userName = user.FullName ?? EmailTemplates.DefaultUserName;

        _emailService.Enqueue(new EmailMessage(
            To: user.Email!,
            Subject: EmailTemplates.OnboardingSubject,
            Title: EmailTemplates.OnboardingTitle,
            Body: EmailTemplates.OnboardingBody,
            UserName: userName,
            ButtonText: EmailTemplates.OnboardingButtonText,
            ButtonUrl: _frontendBaseUrl,
            IllustrationUrl: $"{_assetsBaseUrl}/{EmailTemplates.OnboardingIllustration}"
        ));
    }

    private static string ComputeSha256Hash(string rawData)
    {
        var bytes = Encoding.UTF8.GetBytes(rawData);
        return Convert.ToBase64String(SHA256.HashData(bytes));
    }

    #endregion
}
