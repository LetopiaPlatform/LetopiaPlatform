using System.Text.RegularExpressions;
using HtmlAgilityPack;
using LetopiaPlatform.Core.DTOs.CommunityResourse;
using LetopiaPlatform.Core.Exceptions;
using LetopiaPlatform.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace LetopiaPlatform.Infrastructure.Services;

/// <summary>
/// Scrapes Open Graph metadata from any URL to generate link previews.
/// YouTube thumbnails are resolved directly from the video ID with no HTTP call.
/// Outbound HTTP is performed through SsrfBlockingHandler which
/// rejects connections to private/internal IP ranges.
/// </summary>
public sealed class LinkPreviewService : ILinkPreviewService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<LinkPreviewService> _logger;

    // Matches all YouTube URL variants and captures the 11-char video ID.
    // Supports: youtube.com/watch?v=, /embed/, /shorts/, and youtu.be/
    private static readonly Regex YoutubeRegex = new(
        @"(?:youtube\.com\/(?:watch\?v=|embed\/|shorts\/)|youtu\.be\/)([A-Za-z0-9_\-]{11})",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public LinkPreviewService(HttpClient httpClient, ILogger<LinkPreviewService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<LinkPreviewDto> GetPreviewAsync(string url)
    {
        var ytThumb = TryGetYouTubeThumbnail(url);

        // YouTube: thumbnail from video ID (no HTTP) + scrape title/description normally
        if (ytThumb is not null)
        {
            var meta = await ScrapeMetaAsync(url);
            return meta with { Image = ytThumb };
        }

        return await ScrapeMetaAsync(url);
    }

    // ── Private ───────────────────────────────────────────────────────────────

    private async Task<LinkPreviewDto> ScrapeMetaAsync(string url)
    {
        var safeUrl = SanitizeForLog(url);

        try
        {
            var html = await _httpClient.GetStringAsync(url);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Resolution priority: og: → twitter: → name= fallback
            string? Meta(string key) =>
                OgMeta(doc, $"og:{key}")
                ?? OgMeta(doc, $"twitter:{key}")
                ?? NameMeta(doc, key);

            return new LinkPreviewDto(
                Url: url,
                Title: Meta("title") ?? PlainTitle(doc),
                Description: Meta("description"),
                Image: Meta("image") ?? Meta("image:secure_url"));
        }
        catch (SsrfBlockedException ex)
        {
            _logger.LogWarning(ex, "Blocked SSRF attempt for {Url}", safeUrl);
            return new LinkPreviewDto(url);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "HTTP error scraping preview for {Url}", safeUrl);
            return new LinkPreviewDto(url);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Timeout scraping preview for {Url}", safeUrl);
            return new LinkPreviewDto(url);
        }
        catch (HtmlWebException ex)
        {
            _logger.LogWarning(ex, "HTML parse error scraping preview for {Url}", safeUrl);
            return new LinkPreviewDto(url);
        }
        // NullReferenceException, InvalidOperationException, and other
        // programming errors are intentionally NOT caught here so they
        // surface as bugs rather than being silently swallowed.
    }

    /// <summary>
    /// Strips newlines and truncates to 200 chars to prevent log injection.
    /// The original value is never modified — only used for logging.
    /// </summary>
    private static string SanitizeForLog(string url)
    {
        var safe = url
            .Replace("\r", "", StringComparison.Ordinal)
            .Replace("\n", "", StringComparison.Ordinal);

        return safe.Length > 200 ? safe[..200] : safe;
    }

    /// <summary>
    /// Builds a YouTube maxresdefault (1280×720) thumbnail URL from the
    /// video ID without any network call. Returns null for non-YouTube URLs.
    /// </summary>
    private static string? TryGetYouTubeThumbnail(string url)
    {
        var match = YoutubeRegex.Match(url);
        return match.Success
            ? $"https://img.youtube.com/vi/{match.Groups[1].Value}/maxresdefault.jpg"
            : null;
    }

    private static string? OgMeta(HtmlDocument doc, string property)
    {
        var value = doc.DocumentNode
            .SelectSingleNode($"//meta[@property='{property}']")
            ?.GetAttributeValue("content", string.Empty);

        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static string? NameMeta(HtmlDocument doc, string name)
    {
        var value = doc.DocumentNode
            .SelectSingleNode($"//meta[@name='{name}']")
            ?.GetAttributeValue("content", string.Empty);

        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static string? PlainTitle(HtmlDocument doc) =>
        doc.DocumentNode.SelectSingleNode("//title")?.InnerText?.Trim();
}

public static class LinkPreviewHttpClient
{
    public const string Name = "link-preview";
}
