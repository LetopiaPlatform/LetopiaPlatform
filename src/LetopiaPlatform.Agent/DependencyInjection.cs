using LetopiaPlatform.Agent.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LetopiaPlatform.Agent;

public static class DependencyInjection
{
    public static IServiceCollection AddAgentServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Bind settings
        services.Configure<AgentSettings>(
            configuration.GetSection(AgentSettings.SectionName));
        
        // Register services

        return services;
    }
}