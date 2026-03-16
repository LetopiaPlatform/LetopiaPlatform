namespace LetopiaPlatform.Core.DTOs.Auth.Response;

public record AuthResponse(
    TokenResult JwtToken,
    string RefreshToken,
    UserDto User
);


public record TokenResult(
    string Token,
    DateTime ExpiresAt,
    string Jti
);


public record UserDto(
    string Id,
    string Email,
    string FullName,
    string? Role,
    string? AvatarUrl
);
