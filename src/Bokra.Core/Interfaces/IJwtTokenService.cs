namespace Bokra.Core.Interfaces;

public interface IJwtTokenService
{
    /// <summary>
    /// Generates a JWT token for the given user
    /// </summary>
    /// <param name="user">User to generate token for</param>
    /// <returns>JWT Token string</returns>
    // string GenerateToken(User user);

    /// <summary>
    /// Validates a JWT token and returns whether it is valid
    /// </summary>
    /// <param name="token">JWT Token string</param>
    /// <returns>True if token is valid; otherwise, false</returns>
    bool ValidateToken(string token);
}