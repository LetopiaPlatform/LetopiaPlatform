namespace LetopiaPlatform.Agent.Configuration;

public class AgentSettings
{
    public const string SectionName = "AgentSettings";

    // Primary LLM (Groq)
    public string GroqApiKey { get; set; } = string.Empty;
    public string GroqEndpoint { get; set; } = "https://api.groq.com/openai/v1";
    public string GroqModelId { get; set; } = "llama-3.3-70b-versatile";

    // Fallback LLM (Gemini)
    public string GeminiApiKey { get; set; } = string.Empty;
    public string GeminiEndpoint { get; set; } = "https://generativelanguage.googleapis.com/v1beta/openai/";
    public string GeminiModelId { get; set; } = "gemini-2.5-flash";

    // Web Search
    public string TavilyApiKey { get; set; } = string.Empty;

    // Limits
    public int MaxConversationTokens { get; set; } = 4000;
}
