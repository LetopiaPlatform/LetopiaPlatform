namespace LetopiaPlatform.Core.DTOs.Auth.Request;

public record ResetPasswordRequest(
    string Email,
    string Code,
    string NewPassword
);
