using LetopiaPlatform.Core.DTOs.Auth.Response;
using LetopiaPlatform.Core.Entities.Identity;

namespace LetopiaPlatform.Core.Interfaces;

/// <summary>
/// Generates JSON Web Tokens for authenticated users.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a JWT token for the given user.
    /// </summary>
    /// <param name="user">The user to generate a token for.</param>
    /// <returns>A token result containing the access token and expiration.</returns>
    Task<TokenResult> GenerateTokenAsync(User user);
}