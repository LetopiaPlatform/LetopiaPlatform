namespace LetopiaPlatform.Agent.Configuration;

/// <summary>
/// Configuration settings for AI agent services including LLM providers and runtime limits.
/// </summary>
public class AgentSettings
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "AgentSettings";

    /// <summary>
    /// API key used to authenticate requests to the Groq LLM service.
    /// </summary>
    public string GroqApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Base endpoint URL for the Groq API.
    /// </summary>
    public string GroqEndpoint { get; set; } = "https://api.groq.com/openai/v1";

    /// <summary>
    /// Model identifier used when sending requests to Groq.
    /// </summary>
    public string GroqModelId { get; set; } = "llama-3.3-70b-versatile";

    /// <summary>
    /// API key used to authenticate requests to the Gemini API.
    /// </summary>
    public string GeminiApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Base endpoint URL for the Gemini API.
    /// </summary>
    public string GeminiEndpoint { get; set; } = "https://generativelanguage.googleapis.com/v1beta/openai/";

    /// <summary>
    /// Model identifier used when sending requests to Gemini.
    /// </summary>
    public string GeminiModelId { get; set; } = "gemini-2.5-flash";

    /// <summary>
    /// Maximum number of tokens allowed for a single conversation context.
    /// </summary>
    public int MaxConversationTokens { get; set; } = 4000;

    /// <summary>
    /// Maximum number of tokens the LLM can generate in a single response.
    /// Separate from MaxConversationTokens (context window) to avoid conflation.
    /// </summary>
    public int MaxOutputTokens { get; set; } = 8192;

    /// <summary>
    /// Timeout in seconds for the primary LLM provider's streaming response.
    /// </summary>
    public int PrimaryProviderTimeoutSeconds { get; set; } = 60;
}
