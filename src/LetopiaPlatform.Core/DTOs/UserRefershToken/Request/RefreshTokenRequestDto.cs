namespace LetopiaPlatform.Core.DTOs.UserRefershToken.Request;
public record RefreshTokenRequestDto(
    string AccessToken,
    string RefreshToken
);
