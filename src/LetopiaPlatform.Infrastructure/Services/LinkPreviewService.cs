using System.Text.RegularExpressions;
using System.Xml.XPath;
using HtmlAgilityPack;
using LetopiaPlatform.Core.DTOs.CommunityResourse;
using LetopiaPlatform.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace LetopiaPlatform.Infrastructure.Services;

/// <summary>
/// Scrapes Open Graph metadata from any URL to generate link previews.
/// YouTube thumbnails are resolved directly from the video ID with no HTTP call.
/// </summary>
public class LinkPreviewService : ILinkPreviewService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<LinkPreviewService> _logger;

    // Matches all YouTube URL variants and captures the 11-char video ID.
    // Supports: youtube.com/watch?v=, /embed/, /shorts/, and youtu.be/
    private static readonly Regex YoutubeRegex = new(
        @"(?:youtube\.com\/(?:watch\?v=|embed\/|shorts\/)|youtu\.be\/)([A-Za-z0-9_\-]{11})",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public LinkPreviewService(IHttpClientFactory httpClientFactory, ILogger<LinkPreviewService> logger)
    {
        _httpClient = httpClientFactory.CreateClient(LinkPreviewHttpClient.Name);
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<LinkPreviewDto> GetPreviewAsync(string url)
    {
        // YouTube shortcut: thumbnail is built from the video ID — no HTTP call required.
        // Title and description are still scraped normally.
        var ytThumb = TryGetYouTubeThumbnail(url);
        if (ytThumb is not null)
        {
            var meta = await ScrapeMetaAsync(url);
            return meta with { Image = ytThumb };
        }

        // All other sites (Vimeo, Dailymotion, articles, docs, books, …)
        return await ScrapeMetaAsync(url);
    }

    // ── Private ───────────────────────────────────────────────────────────────

    private async Task<LinkPreviewDto> ScrapeMetaAsync(string url)
    {
        // Sanitize URL for logging — strip newlines to prevent log injection,
        // and cap length so log lines stay readable.
        var safeUrlForLog = url.Replace("\r", "").Replace("\n", "");
        if (safeUrlForLog.Length > 200)
            safeUrlForLog = safeUrlForLog[..200];

        // optional: limit length
        if (safeUrlForLog.Length > 200)
            safeUrlForLog = safeUrlForLog.Substring(0, 200);

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

            return new LinkPreviewDto
            {
                Title = Meta("title") ?? PlainTitle(doc),
                Description = Meta("description"),
                Image = Meta("image") ?? Meta("image:secure_url"),
                Url = url,
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "HTTP error scraping preview for {Url}", safeUrlForLog);
            return new LinkPreviewDto { Url = url };
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Timeout scraping preview for {Url}", safeUrlForLog);
            return new LinkPreviewDto { Url = url };
        }
        catch (HtmlWebException ex)
        {
            _logger.LogWarning(ex, "HTML error scraping preview for {Url}", safeUrlForLog);
            return new LinkPreviewDto { Url = url };
        }
        catch (XPathException ex)
        {
            _logger.LogWarning(ex, "DOM parsing error scraping preview for {Url}", safeUrlForLog);
            return new LinkPreviewDto { Url = url };
        }
        // NullReferenceException, InvalidOperationException, and other
        // programming errors are intentionally NOT caught here so they
        // surface as bugs rather than being silently swallowed.
    }

    /// <summary>
    /// Builds a YouTube <c>maxresdefault</c> (1280×720) thumbnail URL from the
    /// video ID without any network call. Returns <c>null</c> for non-YouTube URLs.
    /// </summary>
    private static string? TryGetYouTubeThumbnail(string url)
    {
        var match = YoutubeRegex.Match(url);
        if (!match.Success) return null;
        return $"https://img.youtube.com/vi/{match.Groups[1].Value}/maxresdefault.jpg";
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

// ── Named HttpClient constants ────────────────────────────────────────────────

/// <summary>
/// Holds the registration name for the <see cref="LinkPreviewService"/> HttpClient.
/// Reference this in DI registration to avoid magic strings.
/// </summary>
public static class LinkPreviewHttpClient
{
    public const string Name = "link-preview";
}


