namespace LetopiaPlatform.Agent.Configuration;

public class AgentSettings
{
    public const string SectionName = "AgentSettings";

    // GitHub Models
    public string GitHubToken { get; set; } = string.Empty;
    public string GitHubModelsEndpoint { get; set; } = "https://models.github.ai/inference";
    public string ModelId { get; set; } = "gpt-4o";

    // Generation parameters
    public float Temperature { get; set; } = 0.7f;
    public int MaxTokens { get; set; } = 2048;

    // External tools
    public string SerperApiKey { get; set; } = string.Empty;
}


