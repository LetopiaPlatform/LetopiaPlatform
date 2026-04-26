using System.ClientModel;
using LetopiaPlatform.Agent.Configuration;
using LetopiaPlatform.Agent.Services;
using LetopiaPlatform.Core.Interfaces;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenAI;

namespace LetopiaPlatform.Agent;

/// <summary>
/// Provides extension methods for setting up dependency injection for the Agent layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers the Agent layer services and configurations into the dependency injection container.
    /// </summary>
    public static IServiceCollection AddAgentServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AgentSettings>(
            configuration.GetSection(AgentSettings.SectionName));

        services.Configure<WebSearchSettings>(
            configuration.GetSection(WebSearchSettings.SectionName));

        // Register Tavily web search service with typed HttpClient
        services.AddHttpClient<TavilySearchService>();
        services.AddScoped<IWebSearchService, TavilySearchService>();

        // Register LLM chat client with primary (Groq) and fallback (Gemini)
        var settings = configuration.GetSection(AgentSettings.SectionName).Get<AgentSettings>()
            ?? throw new InvalidOperationException($"Missing configuration section: {AgentSettings.SectionName}");

        var primaryClient = new OpenAIClient(
                new ApiKeyCredential(settings.GroqApiKey),
                new OpenAIClientOptions { Endpoint = new Uri(settings.GroqEndpoint) })
            .GetChatClient(settings.GroqModelId)
            .AsChatClient();

        var fallbackClient = new OpenAIClient(
                new ApiKeyCredential(settings.GeminiApiKey),
                new OpenAIClientOptions { Endpoint = new Uri(settings.GeminiEndpoint) })
            .GetChatClient(settings.GeminiModelId)
            .AsChatClient();

        services.AddSingleton<IChatClient>(sp =>
            new FallbackChatClient(
                primaryClient,
                fallbackClient,
                sp.GetRequiredService<ILogger<FallbackChatClient>>()));
        services.AddScoped<IRoadmapAgentService, RoadmapAgentService>();

        return services;
    }
}
