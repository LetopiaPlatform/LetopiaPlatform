using LetopiaPlatform.Core.DTOs.Search;

namespace LetopiaPlatform.Core.Interfaces;

/// <summary>
/// Defines a service for performing global searches based on a query and optional filters.
/// </summary>
/// <remarks>Implementations of this interface should support asynchronous search operations and allow callers to
/// specify the maximum number of results and an optional type filter. The service is intended to be used in scenarios
/// where search functionality is required across multiple domains or entities. Thread safety and cancellation support
/// depend on the specific implementation.</remarks>
public interface ISearchService
{
    /// <summary>
    /// Performs an asynchronous global search using the specified query and optional filters.
    /// </summary>
    /// <param name="query">The search query to use for finding matching results. Represents the search term as a string value.</param>
    /// <param name="type">An optional filter specifying the type of results to include. If null, all result types are considered.</param>
    /// <param name="limit">The maximum number of results to return. Must be greater than zero.</param>
    /// <param name="ct">A cancellation token that can be used to cancel the search operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see
    /// cref="GlobalSearchResultDto"/> with the search results matching the specified criteria.</returns>
    Task<GlobalSearchResultDto> SearchAsync(
        string query,
        string? type = null,
        int limit = 5,
        CancellationToken ct = default);
}
