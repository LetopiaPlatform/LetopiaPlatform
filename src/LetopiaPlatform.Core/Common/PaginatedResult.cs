namespace LetopiaPlatform.Core.Common;

/// <summary>
/// Generic paginated result wrapper for list endpoints.
/// Provides pagination metadata alongside the result items.
/// </summary>
/// <typeparam name="T">Type of the items in the paginated result.</typeparam>
public class PaginatedResult<T>
{
    public List<T> Items { get; set;} = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }

    /// <summary>
    /// Creates a <see cref="PaginatedResult{T}"/> from a pre-fetched collection.
    /// with known total items and page parameters.
    /// </summary>
    public static PaginatedResult<T> Create(List<T> items, int totalItems, int page, int pageSize)
    {
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        return new PaginatedResult<T>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        };
    }
}