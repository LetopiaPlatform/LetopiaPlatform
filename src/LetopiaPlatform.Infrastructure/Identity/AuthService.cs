using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.Auth.Request;
using LetopiaPlatform.Core.DTOs.Auth.Response;
using LetopiaPlatform.Core.DTOs.Email;
using LetopiaPlatform.Core.Entities.Identity;
using LetopiaPlatform.Core.Enums;
using LetopiaPlatform.Core.Interfaces;
using LetopiaPlatform.Core.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace LetopiaPlatform.Infrastructure.Identity;

public class AuthService : IAuthService
{
    private const string GoogleProvider = "Google";

    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IGoogleTokenValidator _googleTokenValidator;
    private readonly IEmailService _emailService;

    public AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IJwtTokenService jwtTokenService,
        IGoogleTokenValidator googleTokenValidator,
        IEmailService emailService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
        _googleTokenValidator = googleTokenValidator;
        _emailService = emailService;
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
        {
            return Result.Failure("Failed to assign default role.", 500);
        }   

        // Send Verification code instead of returning JWT
        await SendCodeToUserAsync(user, VerificationPurpose.EmailVerification);

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

        if (!user.EmailVerified)
            return Result<AuthResponse>.Failure("Email not verified. Please verify your email before logging in.", 403);
        
        var tokenResult = await _jwtTokenService.GenerateTokenAsync(user);
        var response = BuildAuthResponse(user, tokenResult);
        return Result<AuthResponse>.Success(response);
    }

    public async Task<Result<AuthResponse>> GoogleLoginAsync(GoogleLoginRequest request, CancellationToken cancellationToken = default)
    {
        var googleUserInfo = await _googleTokenValidator.ValidateAsync(request.AccessToken);
        if (googleUserInfo == null)
        {
            return Result<AuthResponse>.Failure("Invalid Google token.", 401);
        }

        // Check if user exists with Google login linked
        var user = await _userManager.FindByLoginAsync(GoogleProvider, googleUserInfo.GoogleId);
        if (user != null)
        {
            // User exists with Google linked
            var tokenResult = await _jwtTokenService.GenerateTokenAsync(user);
            var response = BuildAuthResponse(user, tokenResult);
            return Result<AuthResponse>.Success(response);
        }

        // Check if user exists by email
        user = await _userManager.FindByEmailAsync(googleUserInfo.Email);
        if (user != null)
        {
            // Link Google account to existing user
            var loginResult = await _userManager.AddLoginAsync(user, new UserLoginInfo(GoogleProvider, googleUserInfo.GoogleId, GoogleProvider));
            if (!loginResult.Succeeded)
            {
                return Result<AuthResponse>.Failure("Failed to link Google account.", 500);
            }

            // Mark email as verified
            user.EmailConfirmed = true;
            user.EmailVerified = true;

            // Import avatar if missing
            if (string.IsNullOrEmpty(user.AvatarUrl) && !string.IsNullOrEmpty(googleUserInfo.PictureUrl))
            {
                user.AvatarUrl = googleUserInfo.PictureUrl;
            }

            user.UpdatedAt = DateTime.UtcNow;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return Result<AuthResponse>.Failure("Failed to update user profile.", 500);
            }

            var tokenResult = await _jwtTokenService.GenerateTokenAsync(user);
            var response = BuildAuthResponse(user, tokenResult);
            return Result<AuthResponse>.Success(response);
        }

        // Create new user
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

        var identityResult = await _userManager.CreateAsync(user);
        if (!identityResult.Succeeded)
        {
            var errors = identityResult.Errors.Select(e => e.Description).ToList();
            return Result<AuthResponse>.Failure(errors, 400);
        }

        // Add external login
        var addLoginResult = await _userManager.AddLoginAsync(user, new UserLoginInfo(GoogleProvider, googleUserInfo.GoogleId, GoogleProvider));
        if (!addLoginResult.Succeeded)
        {
            return Result<AuthResponse>.Failure("Failed to add Google login.", 500);
        }

        // Assign Learner role
        var roleResult = await _userManager.AddToRoleAsync(user, "Learner");
        if (!roleResult.Succeeded)
        {
            return Result<AuthResponse>.Failure("Failed to assign default role.", 500);
        }

        var jwtTokenResult = await _jwtTokenService.GenerateTokenAsync(user);
        var authResponse = BuildAuthResponse(user, jwtTokenResult);

        return Result<AuthResponse>.Success(authResponse, 201);
    }
    
    public async Task<Result> SendVerificationCodeAsync(SendCodeRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Result.Success(); // Prevent email enumeration by returning success even if user doesn't exist

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

        var tokenResult = await _jwtTokenService.GenerateTokenAsync(user);
        var response = BuildAuthResponse(user, tokenResult);
        return Result<AuthResponse>.Success(response);
    }

    public async Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Result.Success(); // Prevent email enumeration

        await SendCodeToUserAsync(user, VerificationPurpose.PasswordReset);
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

    private async Task SendCodeToUserAsync(User user, VerificationPurpose purpose)
    {
        var code = await _userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider);
        
        var (title, subject, body) = purpose switch
        {
            VerificationPurpose.EmailVerification => (
                title: "Verify Your Email",
                subject: "Your Email Verification Code",
                body: $"<p>Your verification code is:</p><h1 style=\"text-align:center; letter-spacing: 8px; font-size: 32px;\">{code}</h1><p>This code expires in 10 minutes.</p>"

            ),
            VerificationPurpose.PasswordReset => (
                title: "Reset Your Password",
                subject: "Your Password Reset Code",
                body: $"<p>Your password reset code is:</p><h1 style=\"text-align:center; letter-spacing: 8px; font-size: 32px;\">{code}</h1><p>This code expires in 10 minutes. If you didn't request this, ignore this email.</p>"
            ),
            _ => throw new ArgumentException("Invalid verification purpose.")
        };

        _emailService.Enqueue(new EmailMessage(
            To: user.Email!,
            Subject: subject,
            Title: title,
            Body: body
        ));
    }

    private static AuthResponse BuildAuthResponse(User user, TokenResult token)
    {
        return new AuthResponse(
            JwtToken: token,
            User: new UserDto(
                Id: user.Id.ToString(),
                Email: user.Email!,
                FullName: user.FullName!,
                Role: user.Role,
                AvatarUrl: user.AvatarUrl
            )
        );
    }
    #endregion
}
