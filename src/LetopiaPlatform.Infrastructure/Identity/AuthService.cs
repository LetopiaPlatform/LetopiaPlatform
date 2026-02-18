using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.Auth.Request;
using LetopiaPlatform.Core.DTOs.Auth.Response;
using LetopiaPlatform.Core.Entities.Identity;
using LetopiaPlatform.Core.Interfaces;
using LetopiaPlatform.Core.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace LetopiaPlatform.Infrastructure.Identity;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result<AuthResponse>> SignUpAsync(SignUpRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            return Result<AuthResponse>.Failure("User with this email already exists.", 409); // Conflict

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

        var tokenResult = await _jwtTokenService.GenerateTokenAsync(user);

        var response = BuildAuthResponse(user, tokenResult);

        return Result<AuthResponse>.Success(response, 201);
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Result<AuthResponse>.Failure("Invalid email or password.", 401);

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!signInResult.Succeeded)
            return Result<AuthResponse>.Failure("Invalid email or password.", 401);

        var tokenResult = await _jwtTokenService.GenerateTokenAsync(user);

        var response = BuildAuthResponse(user, tokenResult);

        return Result<AuthResponse>.Success(response);
    }

    #region Private helpers
    private static AuthResponse BuildAuthResponse(User user, TokenResult token)
    {
        return new AuthResponse(
            JwtToken: token,
            User: new UserDto(
                Id: user.Id.ToString(),
                Email: user.Email!,
                FullName: user.FullName!,
                Role: user.Role 
            )
        );
    }
    #endregion
}
