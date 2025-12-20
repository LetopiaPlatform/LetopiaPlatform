namespace LetopiaPlatform.Core.DTOs.Auth.Request;

public record SignUpRequest
(
    string Email,
    string FullName,
    string PhoneNumber,
    string Password
);
