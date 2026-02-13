namespace LetopiaPlatform.Core.Common;

/// <summary>
/// Base Query DTO for paginated list endpoints.
/// Inherits from this class to add entity-sepcific filters.
/// </summary>
public class PaginatedQuery
{
    /// <summary>
    /// The 1-based page number to retrieve.
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// The number of items per page (1-50)
    /// </summary>
    public int PageSize { get; set; } = 10;
}