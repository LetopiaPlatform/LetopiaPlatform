namespace LetopiaPlatform.Agent.Configuration;

/// <summary>
/// Configuration settings for web search integration.
/// </summary>
public class WebSearchSettings
{
    /// <summary>
    /// Configuration section name used in configuration binding.
    /// </summary>
    public const string SectionName = "WebSearchSettings";

    /// <summary>
    /// API key used to authenticate requests to the Tavily search service.
    /// </summary>
    public string TavilyApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Endpoint URL for the Tavily search API.
    /// </summary>
    public string TavilySearchUrl { get; set; } = "https://api.tavily.com/search";

    /// <summary>
    /// Default maximum number of search results returned by the Tavily API.
    /// </summary>
    public int MaxResults { get; set; } = 5;
}
