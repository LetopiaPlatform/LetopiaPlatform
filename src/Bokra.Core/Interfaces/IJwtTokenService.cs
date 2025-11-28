using Bokra.Core.DTOs.Auth.Response;
using Bokra.Core.Entities.Identity;

namespace Bokra.Core.Interfaces;

public interface IJwtTokenService
{
    /// <summary>
    /// Generates a JWT token for the given user
    /// </summary>
    /// <param name="user">User to generate token for</param>
    /// <returns>JWT Token Result</returns>
    Task<TokenResult> GenerateTokenAsync(User user);
}