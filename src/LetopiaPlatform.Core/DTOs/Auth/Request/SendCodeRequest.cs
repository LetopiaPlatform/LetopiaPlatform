using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.DTOs.Auth.Request;

public record SendCodeRequest(
    string Email,
    VerificationPurpose Purpose);
