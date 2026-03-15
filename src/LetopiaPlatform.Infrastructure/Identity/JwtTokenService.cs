using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using LetopiaPlatform.Core.AppSettings;
using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.Auth.Response;
using LetopiaPlatform.Core.Entities.Identity;
using LetopiaPlatform.Core.Interfaces;
using LetopiaPlatform.Core.Interfaces.Repositories;
using LetopiaPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LetopiaPlatform.Infrastructure.Identity;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly IUnitOfWork<ApplicationDbContext> _unitOfWork;
    private readonly IUserRefreshTokenRepository _userRefreshTokenRepository;
    private readonly IGenericRepository<User> _userRepository;

    public JwtTokenService(
        IOptions<JwtSettings> jwtSettings,
        IUnitOfWork<ApplicationDbContext> unitOfWork,
        IUserRefreshTokenRepository userRefreshTokenRepository,
        IGenericRepository<User> userRepository)
    {
        _jwtSettings = jwtSettings.Value ?? throw new ArgumentNullException(nameof(jwtSettings));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _userRefreshTokenRepository = userRefreshTokenRepository;
        _userRepository = userRepository;
    }

    /// <summary>
    /// Generates a new token pair during login or signup.
    /// </summary>
    public async Task<AuthResponse> GetJWTTokenAsync(User user, CancellationToken ct = default)
    {
        // 1. Cleanup old expired tokens to keep the table size optimized
        await _userRefreshTokenRepository.DeleteExpiredTokensAsync(user.Id, ct);

        // 2. Generate the token pair
        var (tokenResult, userRefreshToken, refreshPlain) = CreateTokenPair(user);

        // 3. Save to database
        await _userRefreshTokenRepository.AddAsync(userRefreshToken);
        await _unitOfWork.SaveChangesAsync(ct);

        return BuildAuthResponse(user, tokenResult, refreshPlain);
    }

    /// <summary>
    /// Refreshes the JWT using a Refresh Token with Concurrency/Race Condition handling.
    /// </summary>
    public async Task<Result<AuthResponse>> RefreshTokenAsync(string accessToken, string refreshToken, CancellationToken ct = default)
    {
        var principal = GetPrincipalFromExpiredToken(accessToken);
        if (principal == null)
            return Result<AuthResponse>.Failure("Invalid access token", 400);

        var userIdClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var jti = principal.FindFirstValue(JwtRegisteredClaimNames.Jti);

        if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(jti) || !Guid.TryParse(userIdClaim, out Guid userId))
            return Result<AuthResponse>.Failure("Invalid token claims", 400);

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return Result<AuthResponse>.Failure("User not found", 404);

        var hash = ComputeSha256Hash(refreshToken);

        // Crucial: Use Tracking to ensure 'IsUsed' concurrency check works via Entity Framework
        var storedToken = await _userRefreshTokenRepository.GetTableAsTracking()
            .FirstOrDefaultAsync(x => x.RefreshTokenHash == hash && x.UserId == userId, ct);

        if (storedToken == null) return Result<AuthResponse>.Failure("Token not found", 404);
        if (storedToken.IsUsed || storedToken.IsRevoked) return Result<AuthResponse>.Failure("Token already used or revoked", 401);
        if (storedToken.JwtId != jti) return Result<AuthResponse>.Failure("Token mismatch", 401);
        if (storedToken.ExpiryDate < DateTime.UtcNow) return Result<AuthResponse>.Failure("Token expired", 401);

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            // 1. Rotation: Invalidate the current token
            storedToken.IsUsed = true;
            await _userRefreshTokenRepository.UpdateAsync(storedToken);

            // 2. Issue: Generate a new pair
            var (tokenResult, newRefreshToken, refreshPlain) = CreateTokenPair(user);
            await _userRefreshTokenRepository.AddAsync(newRefreshToken);

            // 3. Save and Commit
            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitAsync();

            return Result<AuthResponse>.Success(BuildAuthResponse(user, tokenResult, refreshPlain));
        }
        catch (DbUpdateConcurrencyException)
        {
            await _unitOfWork.RollbackAsync();
            return Result<AuthResponse>.Failure("Security Alert: Token is being used simultaneously.", 409);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    // ---------------- Private Helpers ----------------

    private (TokenResult TokenResult, UserRefreshToken RefreshTokenEntity, string RefreshPlain)
        CreateTokenPair(User user)
    {
        var tokenResult = GenerateAccessToken(user);
        var (refreshPlain, refreshExpires) = GenerateRefreshTokenPlain();

        var userRefreshToken = new UserRefreshToken
        {
            UserId = user.Id,
            JwtId = tokenResult.Jti,
            RefreshTokenHash = ComputeSha256Hash(refreshPlain),
            AddedTime = DateTime.UtcNow,
            ExpiryDate = refreshExpires,
            IsUsed = false,
            IsRevoked = false
        };

        return (tokenResult, userRefreshToken, refreshPlain);
    }

    private static AuthResponse BuildAuthResponse(User user, TokenResult tokenResult, string refreshPlain)
        => new AuthResponse(
            JwtToken: tokenResult,
            RefreshToken: refreshPlain,
            User: new UserDto(
                user.Id.ToString(),
                user.Email!,
                user.FullName ?? user.UserName!,
                user.Role ?? "Learner",
                user.AvatarUrl
            )
        );

    private TokenResult GenerateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes);
        var jti = Guid.NewGuid().ToString();

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, jti),
            new Claim(ClaimTypes.Role, user.Role ?? "Learner"),
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds
        );

        return new TokenResult(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt,
            jti);
    }

    private (string TokenString, DateTime ExpiresAt) GenerateRefreshTokenPlain()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        return (Convert.ToBase64String(randomNumber),
                DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryInDays));
    }

    private static string ComputeSha256Hash(string rawText)
    {
        var bytes = Encoding.UTF8.GetBytes(rawText);
        return Convert.ToBase64String(SHA256.HashData(bytes));
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = _jwtSettings.ValidateAudience,
            ValidAudience = _jwtSettings.Audience,
            ValidateIssuer = _jwtSettings.ValidateIssuer,
            ValidIssuer = _jwtSettings.Issuer,
            ValidateIssuerSigningKey = _jwtSettings.ValidateIssuerSigningKey,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
            ValidateLifetime = false
        };

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase))
                return null;

            return principal;
        }
        catch
        {
            return null;
        }
    }
}
