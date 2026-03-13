namespace LetopiaPlatform.API.DTOs.Auth.Request;

public record VerifyEmailDto(
    string Email,
    string Code);
