using LetopiaPlatform.API.AppMetaData;
using LetopiaPlatform.API.DTOs.Auth.Request;
using LetopiaPlatform.API.Extensions;
using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.Auth.Request;
using LetopiaPlatform.Core.Enums;
using LetopiaPlatform.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace LetopiaPlatform.API.Controllers;

[ApiController]
[EnableRateLimiting(RateLimitingExtensions.AuthPolicy)]
public class AuthController : BaseController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost(Router.Authentication.SignUp)]
    public async Task<IActionResult> SignUp([FromBody] SignUpDto request)
    {
        HttpContext.AddBusinessContext("action", "signup");
        HttpContext.AddBusinessContext("email", request.Email);

        var result = await _authService.SignUpAsync(new SignUpRequest(
            Email: request.Email,
            FullName: request.FullName,
            PhoneNumber: request.PhoneNumber,
            Password: request.Password
        ));

        return HandleResult(result);
    }

    [HttpPost(Router.Authentication.Login)]
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        HttpContext.AddBusinessContext("action", "login");
        HttpContext.AddBusinessContext("email", request.Email);

        var result = await _authService.LoginAsync(new LoginRequest(
            Email: request.Email,
            Password: request.Password
        ));

        if (result.IsSuccess)
            HttpContext.AddBusinessContext("login_user_id", result.Value!.User.Id);

        return HandleResult(result);
    }

    [HttpPost(Router.Authentication.SendCode)]
    public async Task<IActionResult> SendCode([FromBody] SendCodeDto request)
    {
        HttpContext.AddBusinessContext("action", "send_code");

        if (!Enum.TryParse<OtpPurpose>(request.Purpose, out var purpose))
            return HandleResult(Result.Failure("Invalid purpose. Must be 'EmailVerification' or 'PasswordReset'."));

        var result = await _authService.SendVerificationCodeAsync(new SendCodeRequest(
            Email: request.Email,
            Purpose: purpose
        ), HttpContext.RequestAborted);

        return HandleResult(result);
    }

    [HttpPost(Router.Authentication.VerifyEmail)]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto request)
    {
        HttpContext.AddBusinessContext("action", "verify_email");

        var result = await _authService.VerifyEmailAsync(new VerifyEmailRequest(
            Email: request.Email,
            Code: request.Code
        ), HttpContext.RequestAborted);

        return HandleResult(result);
    }

    [HttpPost(Router.Authentication.ForgotPassword)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto request)
    {
        HttpContext.AddBusinessContext("action", "forgot_password");

        var result = await _authService.ForgotPasswordAsync(new ForgotPasswordRequest(
            Email: request.Email
        ), HttpContext.RequestAborted);

        return HandleResult(result);
    }

    [HttpPost(Router.Authentication.ResetPassword)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
    {
        HttpContext.AddBusinessContext("action", "reset_password");

        var result = await _authService.ResetPasswordAsync(new ResetPasswordRequest(
            Email: request.Email,
            Code: request.Code,
            NewPassword: request.NewPassword
        ), HttpContext.RequestAborted);

        return HandleResult(result);
    }

    [HttpPost(Router.Authentication.GoogleLogin)]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        HttpContext.AddBusinessContext("action", "google_login");
        
        var result = await _authService.GoogleLoginAsync(request);

        if (result.IsSuccess)
        {
            HttpContext.AddBusinessContext("google_login_user_id", result.Value!.User.Id);
        }
        
        return HandleResult(result);

    }
}
