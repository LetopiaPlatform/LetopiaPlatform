namespace LetopiaPlatform.Core.Interfaces;

/// <summary>
/// Marker interface for entities supporting soft deletion.
/// <para>
/// Entities implementing this interface will have a global query filter applied automatically by <c>ApplicationDbContext</c>, execluding records where <c>IsDeleted == true</c> from all standard queries.
/// </para>
/// <para>
/// Use <c>.IgnoreQueryFilters()</c> to include soft-deleted records (e.g., moderation dashboards, admin views).
/// </para>
/// </summary>
public interface ISoftDeletable
{
    /// <summary>
    /// Indicates whether the entity has been soft-deleted.
    /// </summary>
    bool IsDeleted { get; set; }
}