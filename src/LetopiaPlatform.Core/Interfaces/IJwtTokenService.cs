using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.Auth.Response;
using LetopiaPlatform.Core.Entities.Identity;

namespace LetopiaPlatform.Core.Interfaces;

/// <summary>
/// Service for managing JSON Web Tokens, including generation, validation, and refresh logic.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a complete authentication response (Access Token + Refresh Token) for a user.
    /// Typically used during initial Login or Registration.
    /// </summary>
    Task<AuthResponse> GetJWTTokenAsync(User user, CancellationToken ct = default);

    /// <summary>
    /// Rotates the current tokens by validating the expired access token and the provided refresh token.
    /// Implements security checks like Reuse Detection and Expiry validation.
    /// </summary>
    Task<Result<AuthResponse>> RefreshTokenAsync(string accessToken, string refreshToken, CancellationToken ct = default);

    /// <summary>
    /// Extracts the ClaimsPrincipal from an expired JWT token for validation purposes.
    /// </summary>
    System.Security.Claims.ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
