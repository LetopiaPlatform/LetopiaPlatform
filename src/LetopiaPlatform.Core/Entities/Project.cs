using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.Entities.Identity;
using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.Entities;
public class Project : AuditableEntity
{
    public required string Title { get; set; }
    public required string Description { get; set; }


    public DifficultyLevel? DifficultyLevel { get; set; }

    public DateTime Deadline { get; set; }

    public bool IsFull { get; set; }
    //-------------
    // ── التواريخ (Timeline) ────────────────────────
    public DateTime StartDate { get; set; }

    // ── الإحصائيات (UI Data) ────────────────────────
    public int ProgressPercentage { get; set; }

    // ── القوائم (Lists) ─────────────────────────────
    public List<string> Goals { get; set; } = [];



    //-----------------------
    public ProjectStatus Status { get; set; }


    public int MaxMembers { get; set; } = 5;

    public List<string> RequiredSkills { get; set; } = [];

    public string? CoverImageUrl { get; set; }



    public Guid CategoryId { get; set; }

    public virtual ProjectCategory Category { get; set; } = null!;

    // صاحب المشروع
    public Guid OwnerId { get; set; }
    public virtual User Owner { get; set; } = null!;

    public virtual ICollection<ProjectMember> Members { get; set; } = new HashSet<ProjectMember>();
}
