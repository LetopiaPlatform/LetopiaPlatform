namespace LetopiaPlatform.API.DTOs.Auth.Request;

public record SignUpDto
(
    string Email,
    string FullName,
    string PhoneNumber,
    string Password,
    string ConfirmPassword
);
