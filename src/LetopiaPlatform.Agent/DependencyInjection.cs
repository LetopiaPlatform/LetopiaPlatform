using LetopiaPlatform.Agent.Configuration;
using LetopiaPlatform.Agent.Services;
using LetopiaPlatform.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LetopiaPlatform.Agent;

/// <summary>
/// Registers agent-layer services into the dependency injection container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds agent services including LLM configuration and web search integration.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <returns>The configured service collection for chaining.</returns>
    public static IServiceCollection AddAgentServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AgentSettings>(configuration.GetSection(AgentSettings.SectionName));

        // Tavily web search
        services.AddHttpClient<TavilySearchService>();
        services.AddScoped<IWebSearchService, TavilySearchService>();

        return services;
    }
}
