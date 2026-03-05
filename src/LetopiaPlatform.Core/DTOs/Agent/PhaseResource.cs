using LetopiaPlatform.Core.Enums;
namespace LetopiaPlatform.Core.DTOs.Agent;

public class PhaseResource
{
    public required string Title { get; set; }
    public required string Url { get; set; }
    public ResourceType Type { get; set; }
    public string? Provider { get; set; }
    public bool IsFree { get; set; }
}
