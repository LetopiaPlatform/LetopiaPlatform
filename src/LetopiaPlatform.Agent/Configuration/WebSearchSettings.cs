namespace LetopiaPlatform.Agent.Configuration;

/// <summary>
/// Configuration settings for web search integration.
/// Contains API keys and other related settings for web search services.
/// </summary>
public class WebSearchSettings
{
    /// <summary>
    /// The configuration section name used in appsettings.json.
    /// </summary>
    public const string SectionName = "WebSearchSettings";

    /// <summary>
    /// The API key for the Tavily search service.
    /// </summary>
    public string TavilyApiKey { get; set; } = string.Empty;
}
