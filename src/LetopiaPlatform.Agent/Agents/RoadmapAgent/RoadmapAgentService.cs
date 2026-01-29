using LetopiaPlatform.Agent.Abstractions;
using LetopiaPlatform.Agent.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Chat;

namespace LetopiaPlatform.Agent.Agents.RoadmapAgent;

public class RoadmapAgentService : BaseAgentService
{
    public RoadmapAgentService(
        IOptions<AgentSettings> options,
        ILogger<RoadmapAgentService> logger,
        IConversationCache conversationCache)
        : base(options, logger, conversationCache)
    {
    }

    public override string AgentType => "roadmap";

    protected override string GetSystemPrompt()
    {
        return """
               You are an expert AI Roadmap Planner.
               Your job is to help users create detailed, structured, and actionable roadmaps.
               Break goals into phases, milestones, tasks, and timelines.
               Ask clarifying questions when requirements are ambiguous.
               """;
    }

    protected override IEnumerable<ChatTool> GetTools()
    {
        return RoadmapTools.GetTools();
    }
}
