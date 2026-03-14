using System.Net.Http.Headers;
using System.Text.Json;
using LetopiaPlatform.Core.AppSettings;
using LetopiaPlatform.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LetopiaPlatform.Infrastructure.Identity;

public class GoogleTokenValidator : IGoogleTokenValidator
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly GoogleAuthSettings _settings;
    private readonly ILogger<GoogleTokenValidator> _logger;

    public GoogleTokenValidator(
        IHttpClientFactory httpClientFactory,
        IOptions<GoogleAuthSettings> settings,
        ILogger<GoogleTokenValidator> logger)
    {
        _httpClientFactory = httpClientFactory;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<GoogleUserInfo?> ValidateAsync(string accessToken)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();

            // Verify audience — ensure the token was issued for our app
            var tokenInfoResponse = await client.GetAsync(
                $"https://oauth2.googleapis.com/tokeninfo?access_token={accessToken}");

            if (!tokenInfoResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning("Google tokeninfo request failed with status {StatusCode}", tokenInfoResponse.StatusCode);
                return null;
            }

            var tokenInfoJson = await tokenInfoResponse.Content.ReadAsStringAsync();

            using var tokenInfoDoc = JsonDocument.Parse(tokenInfoJson);
            var root = tokenInfoDoc.RootElement;
            var aud = root.TryGetProperty("aud", out var audProp) ? audProp.GetString() : null;

            if (aud != _settings.ClientId)
            {
                _logger.LogWarning("Google token audience mismatch. Expected {Expected}, got {Actual}",
                    _settings.ClientId, aud);
                return null;
            }

            // Get user info
            var request = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/oauth2/v3/userinfo");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Google userinfo request failed with status {StatusCode}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var userInfo = JsonSerializer.Deserialize<GoogleUserInfoResponse>(json);

            if (userInfo is null || string.IsNullOrEmpty(userInfo.Sub) || string.IsNullOrEmpty(userInfo.Email))
            {
                _logger.LogWarning("Google userinfo response missing required fields");
                return null;
            }

            return new GoogleUserInfo(
                GoogleId: userInfo.Sub,
                Email: userInfo.Email,
                Name: userInfo.Name ?? userInfo.Email,
                PictureUrl: userInfo.Picture
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Google access token validation failed");
            return null;
        }
    }

    private sealed record GoogleUserInfoResponse(
        [property: System.Text.Json.Serialization.JsonPropertyName("sub")] string? Sub,
        [property: System.Text.Json.Serialization.JsonPropertyName("email")] string? Email,
        [property: System.Text.Json.Serialization.JsonPropertyName("name")] string? Name,
        [property: System.Text.Json.Serialization.JsonPropertyName("picture")] string? Picture,
        [property: System.Text.Json.Serialization.JsonPropertyName("email_verified")] bool? EmailVerified
    );
}