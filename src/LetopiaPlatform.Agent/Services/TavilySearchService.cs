using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using LetopiaPlatform.Agent.Configuration;
using LetopiaPlatform.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LetopiaPlatform.Agent.Services;

/// <summary>
/// Integrates with the Tavily Search API to provide web search capabilities
/// for finding real learning resources.
/// </summary>
/// <remarks>
/// Uses a typed <see cref="HttpClient"/> injected via <c>IHttpClientFactory</c>.
/// On any failure (network, deserialization, HTTP error), the service logs a warning
/// and returns an empty list — it never throws exceptions to the caller.
/// </remarks>
public class TavilySearchService : IWebSearchService
{
    private const string TavilySearchUrl = "https://api.tavily.com/search";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _httpClient;
    private readonly AgentSettings _settings;
    private readonly ILogger<TavilySearchService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TavilySearchService"/> class.
    /// </summary>
    /// <param name="httpClient">The typed HTTP client for Tavily API calls.</param>
    /// <param name="settings">Agent configuration containing the Tavily API key.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    public TavilySearchService(
        HttpClient httpClient,
        IOptions<AgentSettings> settings,
        ILogger<TavilySearchService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Sends a POST request to the Tavily Search API and maps the response
    /// into a list of <see cref="SearchResult"/> records.
    /// </remarks>
    public async Task<List<SearchResult>> SearchAsync(
        string query, int maxResults = 5, CancellationToken ct = default)
    {
        try
        {
            var requestBody = new TavilySearchRequest
            {
                Query = query,
                MaxResults = maxResults,
                ApiKey = _settings.TavilyApiKey
            };

            using var response = await _httpClient.PostAsJsonAsync(
                TavilySearchUrl, requestBody, JsonOptions, ct).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var tavilyResponse = await response.Content
                .ReadFromJsonAsync<TavilySearchResponse>(JsonOptions, ct)
                .ConfigureAwait(false);

            if (tavilyResponse?.Results is null)
            {
                _logger.LogWarning("Tavily API returned null results for query: {Query}", query);
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
            _logger.LogWarning(ex, "Tavily search failed for query: {Query}", query);
            return [];
        }
    }

    #region Tavily API DTOs

    /// <summary>
    /// Represents the request body sent to the Tavily Search API.
    /// </summary>
    private sealed class TavilySearchRequest
    {
        /// <summary>Gets or sets the search query string.</summary>
        public string Query { get; set; } = string.Empty;

        /// <summary>Gets or sets the maximum number of results to return.</summary>
        public int MaxResults { get; set; }

        /// <summary>Gets or sets the Tavily API key.</summary>
        public string ApiKey { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents the response received from the Tavily Search API.
    /// </summary>
    private sealed class TavilySearchResponse
    {
        /// <summary>Gets or sets the list of search results.</summary>
        public List<TavilyResult>? Results { get; set; }
    }

    /// <summary>
    /// Represents a single result item from the Tavily Search API response.
    /// </summary>
    private sealed class TavilyResult
    {
        /// <summary>Gets or sets the title of the search result.</summary>
        public string? Title { get; set; }

        /// <summary>Gets or sets the URL of the search result.</summary>
        public string? Url { get; set; }

        /// <summary>Gets or sets the content snippet of the search result.</summary>
        public string? Content { get; set; }
    }

    #endregion
}
