namespace LetopiaPlatform.Core.Interfaces;

/// <summary>
/// Defines a contract for validating Google ID tokens and extracting user information from them.
/// </summary>
public interface IGoogleTokenValidator
{
    /// <summary>
    /// Validates the provided Google ID token and extracts user information.
    /// </summary>
    /// <param name="accessToken">The access token received from the client after Google OAuth authentication.</param>
    /// <returns>A result containing the extracted user information if the token is valid.</returns>
    Task<GoogleUserInfo?> ValidateAsync(string accessToken);
}

/// <summary>
/// Represents user information extracted from a valid Google ID token.
/// </summary>
/// <param name="GoogleId">The unique Google identifier for the user.</param>
/// <param name="Email">The user's email address.</param>
/// <param name="Name">The user's display name.</param>
/// <param name="PictureUrl">The URL to the user's profile picture, if available.</param>
public record GoogleUserInfo(
    string GoogleId,
    string Email,
    string Name,
    string? PictureUrl
);