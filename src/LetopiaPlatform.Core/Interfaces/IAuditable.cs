namespace LetopiaPlatform.Core.Interfaces;

/// <summary>
/// Contract for entities with automatic audit timestamps.
/// <para>
/// <c>CreatedAt</c> is set on insert; <c>UpdatedAt</c> is set on every save.
/// Both are populated automatically by the <c>SaveChangesAsync</c> override
/// in <c>ApplicationDbContext</c>.
/// </para>
/// <para>
/// Implemented by <c>AuditableEntity</c> (for standard entities) and directly
/// by <c>User</c> (which inherits from <c>IdentityUser</c> and cannot use
/// <c>AuditableEntity</c> as a base class).
/// </para>
/// </summary>
public interface IAuditable
{
    /// <summary>
    /// The UTC timestamp when the entity was created.
    /// </summary>
    DateTime CreatedAt { get; set; }

    /// <summary>
    /// The UTC timestamp when the entity was last updated.
    /// </summary>
    DateTime UpdatedAt { get; set; }
}
