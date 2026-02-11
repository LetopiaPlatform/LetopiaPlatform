
using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.Auth.Request;
using LetopiaPlatform.Core.DTOs.Auth.Response;

namespace LetopiaPlatform.Core.Services.Interfaces;

/// <summary>
/// Handles user authentication operations including registration and sign-in.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user account and returns an authentication token.
    /// </summary>
    /// <param name="request">The sign-up details including email, password, and profile info.</param>
    /// <returns>A result containing the authentication response with JWT token on success.</returns>
    Task<Result<AuthResponse>> SignUpAsync(SignUpRequest request);

    /// <summary>
    /// Authenticates a user with their credentials and returns an authentication token.
    /// </summary>
    /// <param name="request">The login credentials (email and password).</param>
    /// <returns>A result containing the authentication response with JWT token on success.</returns>
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request);
}
