using LetopiaPlatform.Core.Enums;
namespace LetopiaPlatform.Core.DTOs.Agent;

public class PhaseProject
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string Difficulty { get; set; }
    public List<string> Milestones { get; set; } = [];
}
