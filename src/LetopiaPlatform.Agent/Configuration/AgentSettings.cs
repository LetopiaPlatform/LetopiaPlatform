namespace LetopiaPlatform.Agent.Configuration;

public class AgentSettings
{
    public const string SectionName = "AgentSettings";
    
    public string GitHubToken { get; set; } = string.Empty;
    public string ModelId { get; set; } = "gpt-4o";
    public string GitHubModelsEndpoint { get; set; } = "https://models.github.ai/inference";
    public string SerperApiKey { get; set; } = string.Empty;
}