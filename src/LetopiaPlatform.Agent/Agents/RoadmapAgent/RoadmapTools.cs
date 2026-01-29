using OpenAI.Chat;

namespace LetopiaPlatform.Agent.Agents.RoadmapAgent;

public static class RoadmapTools
{
    public static IEnumerable<ChatTool> GetTools()
    {
        // No tools for now – will be added later (e.g. WebSearch, Planner, Memory, etc.)
        return Enumerable.Empty<ChatTool>();
    }
}
