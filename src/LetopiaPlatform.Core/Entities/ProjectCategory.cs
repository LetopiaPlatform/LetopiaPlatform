using LetopiaPlatform.Core.Common;

namespace LetopiaPlatform.Core.Entities;
public class ProjectCategory : BaseEntity
{
    public required string Name { get; set; }

    public required string Slug { get; set; }


    public int DisplayOrder { get; set; }


    public virtual ICollection<Project> Projects { get; set; } = new HashSet<Project>();
}
