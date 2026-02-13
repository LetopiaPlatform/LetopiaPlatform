namespace LetopiaPlatform.Core.Common;

/// <summary>
/// Base class for all domain entities providing a consistent identifier.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; }
}
