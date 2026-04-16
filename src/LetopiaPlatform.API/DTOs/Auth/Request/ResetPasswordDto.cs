namespace LetopiaPlatform.API.DTOs.Auth.Request;

public record ResetPasswordDto(
    string Email,
    string Code,
    string NewPassword,
    string ConfirmPassword);
