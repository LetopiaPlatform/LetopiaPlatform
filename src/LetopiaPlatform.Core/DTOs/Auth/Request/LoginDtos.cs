namespace LetopiaPlatform.Core.DTOs.Auth.Request;

public record LoginRequest
(
    string Email,
    string Password
);
