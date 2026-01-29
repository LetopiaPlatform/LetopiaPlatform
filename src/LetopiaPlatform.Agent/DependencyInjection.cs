using LetopiaPlatform.Agent.Abstractions;
using LetopiaPlatform.Agent.Configuration;
using LetopiaPlatform.Agent.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LetopiaPlatform.Agent;

public static class DependencyInjection
{
    public static IServiceCollection AddAgentServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AgentSettings>(
            configuration.GetSection(AgentSettings.SectionName));

        services.AddSingleton<IConversationCache, InMemoryConversationCache>();

        return services;
    }
}
