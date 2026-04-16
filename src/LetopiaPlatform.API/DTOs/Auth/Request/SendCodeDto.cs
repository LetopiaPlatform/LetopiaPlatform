namespace LetopiaPlatform.API.DTOs.Auth.Request;

public record SendCodeDto(
    string Email,
    string Purpose
);
