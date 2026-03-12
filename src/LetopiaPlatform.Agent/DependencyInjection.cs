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
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configuration">The application <see cref="IConfiguration"/> instance.</param>
    /// <returns>The original <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddAgentServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AgentSettings>(
            configuration.GetSection(AgentSettings.SectionName));

        services.Configure<WebSearchSettings>(
            configuration.GetSection(WebSearchSettings.SectionName));

        // Register Tavily web search service with typed HttpClient
        services.AddHttpClient<IWebSearchService, TavilySearchService>();

        return services;
    }
}
