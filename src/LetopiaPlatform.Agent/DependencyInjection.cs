using LetopiaPlatform.Agent.Configuration;
<<<<<<< HEAD
using LetopiaPlatform.Agent.Services;
using LetopiaPlatform.Core.Interfaces;
=======
>>>>>>> main
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LetopiaPlatform.Agent;

/// <summary>
<<<<<<< HEAD
/// Registers agent-layer services into the dependency injection container.
=======
/// Provides extension methods for setting up dependency injection for the Agent layer.
>>>>>>> main
/// </summary>
public static class DependencyInjection
{
    /// <summary>
<<<<<<< HEAD
    /// Adds agent services including LLM configuration and web search integration.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <returns>The configured service collection for chaining.</returns>
=======
    /// Registers the Agent layer services and configurations into the dependency injection container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configuration">The application <see cref="IConfiguration"/> instance.</param>
    /// <returns>The original <see cref="IServiceCollection"/> for chaining.</returns>
>>>>>>> main
    public static IServiceCollection AddAgentServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AgentSettings>(configuration.GetSection(AgentSettings.SectionName));
<<<<<<< HEAD

        // Tavily web search
        services.AddHttpClient<TavilySearchService>();
        services.AddScoped<IWebSearchService, TavilySearchService>();

=======
        services.Configure<WebSearchSettings>(configuration.GetSection(WebSearchSettings.SectionName));
>>>>>>> main
        return services;
    }
}
