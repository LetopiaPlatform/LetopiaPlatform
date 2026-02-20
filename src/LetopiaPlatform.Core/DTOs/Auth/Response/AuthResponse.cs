namespace LetopiaPlatform.Core.DTOs.Auth.Response;

public record AuthResponse(
  TokenResult JwtToken,
  UserDto User
);

public record TokenResult(
    string Token,
    DateTime ExpiresAt
);

public record UserDto(
    string Id,
    string Email,
    string FullName,
    string Role
);
