using LetopiaPlatform.Agent.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace LetopiaPlatform.Agent;

/// <summary>
/// Factory interface for creating and retrieving agent services.
/// </summary>
public interface IAgentFactory
{
    /// <summary>
    /// Gets an agent service instance by its type.
    /// </summary>
    /// <param name="agentType">The type of the agent to retrieve.</param>
    /// <returns>An <see cref="IAgentService"/> instance corresponding to the specified agent type.</returns>
    IAgentService GetAgent(string agentType);

    /// <summary>Get all available agent types</summary>
    IEnumerable<string> GetAvailableAgents();
}

public class AgentFactory : IAgentFactory
{
    private readonly IServiceProvider _serviceProvider;
    
    // Registry of available agents - add new agents here
    private static readonly Dictionary<string, Type> AgentRegistry = new(StringComparer.OrdinalIgnoreCase)
    {
        // ["roadmapAgent"] = typeof(RoadmapAgentService), // Future
        // ["community"] = typeof(CommunityAgentService), // Future
        // ["studyBuddy"] = typeof(StudyBuddyAgentService), // Future
    };
    
    public AgentFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public IAgentService GetAgent(string agentType)
    {
        if (!AgentRegistry.TryGetValue(agentType, out var serviceType))
        {
            throw new ArgumentException(
                $"Unknown agent type: '{agentType}'. Available agents: {string.Join(", ", AgentRegistry.Keys)}");
        }
        
        return (IAgentService)_serviceProvider.GetRequiredService(serviceType);
    }
    
    public IEnumerable<string> GetAvailableAgents() => AgentRegistry.Keys;
}