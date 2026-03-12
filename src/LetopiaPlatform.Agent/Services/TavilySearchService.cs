using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using LetopiaPlatform.Agent.Configuration;
using LetopiaPlatform.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LetopiaPlatform.Agent.Services;

/// <summary>
/// Provides web search capabilities by integrating with the Tavily Search API.
/// </summary>
public class TavilySearchService : IWebSearchService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _httpClient;
    private readonly WebSearchSettings _settings;
    private readonly ILogger<TavilySearchService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TavilySearchService"/> class.
    /// </summary>
    /// <param name="httpClient">Typed HTTP client used to communicate with the Tavily API.</param>
    /// <param name="settings">Web search configuration settings.</param>
    /// <param name="logger">Logger instance.</param>
    public TavilySearchService(
        HttpClient httpClient,
        IOptions<WebSearchSettings> settings,
        ILogger<TavilySearchService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Executes a search query against the Tavily API and returns structured results.
    /// </summary>
    /// <param name="query">Search query.</param>
    /// <param name="maxResults">Maximum number of results requested.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of mapped <see cref="SearchResult"/> objects.</returns>
        public async Task<List<SearchResult>> SearchAsync(
            string query,
            int maxResults = 0,
            CancellationToken ct = default)
        {
            try
            {
                var resultsLimit = maxResults > 0 ? maxResults : _settings.MaxResults;

                var requestBody = new TavilySearchRequest
                {
                    Query = query,
                    MaxResults = resultsLimit,
                    ApiKey = _settings.TavilyApiKey
                };

                using var response = await _httpClient.PostAsJsonAsync(
                    _settings.TavilySearchUrl,
                    requestBody,
                    JsonOptions,
                    ct).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var tavilyResponse = await response.Content
                .ReadFromJsonAsync<TavilySearchResponse>(JsonOptions, ct)
                .ConfigureAwait(false);

            if (tavilyResponse?.Results is null)
            {
                _logger.LogWarning(
                    "Tavily API returned null results for query: {Query}",
                    query);

                return [];
            }

            return tavilyResponse.Results
                .Select(r => new SearchResult(
                    Title: r.Title ?? string.Empty,
                    Url: r.Url ?? string.Empty,
                    Snippet: r.Content ?? string.Empty))
                .ToList();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(
                ex,
                "Tavily search failed for query: {Query}",
                query);

            return [];
        }
    }

    private sealed class TavilySearchRequest
    {
        public string Query { get; set; } = string.Empty;
        public int MaxResults { get; set; }
        public string ApiKey { get; set; } = string.Empty;
    }

    private sealed class TavilySearchResponse
    {
        public List<TavilyResult>? Results { get; set; }
    }

    private sealed class TavilyResult
    {
        public string? Title { get; set; }
        public string? Url { get; set; }
        public string? Content { get; set; }
    }
}
