namespace LetopiaPlatform.Core.DTOs.Auth.Response;

public record AuthResponse(
    TokenResult JwtToken,
    string RefreshToken,
    UserDto User
);

// نتيجة توليد الـ JWT Access Token
public record TokenResult(
    string Token,
    DateTime ExpiresAt,
    string Jti
);

// بيانات اليوزر الأساسية
public record UserDto(
    string Id,
    string Email,
    string FullName,
    string? Role,
    string? AvatarUrl
);
