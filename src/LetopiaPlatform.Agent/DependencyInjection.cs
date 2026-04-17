using LetopiaPlatform.Agent.Configuration;
using LetopiaPlatform.Agent.Services;
using LetopiaPlatform.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        return services;
    }
}
