namespace LetopiaPlatform.Core.Common;

/// <summary>
/// Extends <see cref="BaseEntity"/> with automatic audit timestamps.
/// <para>
/// <c> CreatedAt</c> is set on insert; <c>UpdatedAt</c> is set on every update.
/// Both are populated automatically by the <c>SaveChangesAsync</c> override in the <c>ApplicationDbContext</c>.
/// </para>
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
