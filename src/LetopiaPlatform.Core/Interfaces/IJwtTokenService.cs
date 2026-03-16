using System.Security.Claims;
using LetopiaPlatform.Core.Entities.Identity;

namespace LetopiaPlatform.Core.Interfaces;

/// <summary>
/// Service for managing JSON Web Tokens, including generation, validation, and refresh logic.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a JWT Access Token for the specified user.
    /// Following SRP: Only handles Access Token generation.
    /// </summary>
    string GenerateJwtToken(User user);

    /// <summary>
    /// Generates a secure random string to be used as a Refresh Token.
    /// Following SRP: Only handles the raw string generation.
    /// </summary>
    string GenerateRefreshToken();
    /// <summary>
    /// 
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
