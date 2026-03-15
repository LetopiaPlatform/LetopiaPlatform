using LetopiaPlatform.API.AppMetaData;
using LetopiaPlatform.API.DTOs.Auth.Request;
using LetopiaPlatform.API.Extensions;
using LetopiaPlatform.Core.DTOs.Auth.Request;
using LetopiaPlatform.Core.DTOs.UserRefershToken.Request;
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
        // Enrich the wide event business context
        HttpContext.AddBusinessContext("action", "signup");
        HttpContext.AddBusinessContext("email", request.Email);

        // Map API DTO to Core DTO inline
        var result = await _authService.SignUpAsync(new SignUpRequest(
            Email: request.Email,
            FullName: request.FullName,
            PhoneNumber: request.PhoneNumber,
            Password: request.Password
        ));

        if (result.IsSuccess)
        {
            HttpContext.AddBusinessContext("new_user_id", result.Value!.User.Id);
        }

        return HandleResult(result);
    }

    [HttpPost(Router.Authentication.Login)]
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        // Enrich the wide event business context
        HttpContext.AddBusinessContext("action", "login");
        HttpContext.AddBusinessContext("email", request.Email);

        var result = await _authService.LoginAsync(new LoginRequest(
            Email: request.Email,
            Password: request.Password
        ));

        if (result.IsSuccess)
        {
            HttpContext.AddBusinessContext("login_user_id", result.Value!.User.Id);
        }

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

    [HttpPost(Router.Authentication.GenerateAccessTokenFromRefreshToken)]
    public async Task<IActionResult> GenerateAccessTokenFromRefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        // 1. Enrich the wide event business context for logging/telemetry
        HttpContext.AddBusinessContext("action", "refresh_token");

        // 2. Call the service to rotate the tokens
        var result = await _authService.RefreshTokenAsync(request);

        // 3. If rotation succeeded, capture the user context
        if (result.IsSuccess)
        {
            HttpContext.AddBusinessContext("user_id", result.Value!.User.Id);
        }

        // 4. Standardized response handling via BaseController
        return HandleResult(result);
    }
}
