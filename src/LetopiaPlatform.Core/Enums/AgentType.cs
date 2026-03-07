
using System.Text.Json.Serialization;

namespace LetopiaPlatform.Core.Enums;

/// <summary>
/// Defines the types of AI agents available in the platform.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AgentType
{
    /// <summary>
    /// Agent responsible for generating personalized learning roadmaps.
    /// </summary>
    RoadmapGenerator
}
