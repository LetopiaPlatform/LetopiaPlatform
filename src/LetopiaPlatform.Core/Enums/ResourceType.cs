using System.Text.Json.Serialization;

namespace LetopiaPlatform.Core.Enums;

/// <summary>
/// Represents the type of a learning resource.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ResourceType
{
    Course,
    Article,
    Documentation,
    Book,
    Video,
    Tool
}
