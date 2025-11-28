namespace Bokra.Core.DTOs.Auth.Response;

public record AuthResponse(
  TokenResult jwtToken,
  UserDto user
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
