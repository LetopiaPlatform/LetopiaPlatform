namespace LetopiaPlatform.Core.DTOs.Auth.Request;

public record VerifyEmailRequest(
    string Email,
    string Code);
