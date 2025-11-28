using Bokra.API.AppMetaData;
using Bokra.API.DTOs.Auth.Request;
using Bokra.Core.DTOs.Auth.Request;
using Bokra.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Bokra.API.Controllers;

[ApiController]
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
        // Map API DTO to Core DTO inline
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
        var result = await _authService.LoginAsync(new LoginRequest(
            Email: request.Email,
            Password: request.Password
        ));

        return HandleResult(result);
    }
}
