namespace LetopiaPlatform.Core.Interfaces;

/// <summary>
/// Performs web searches to retrieve relevant online resources.
/// </summary>
public interface IWebSearchService
{
    /// <summary>
    /// Searches the web for the specified query and returns matching results.
    /// </summary>
    /// <param name="query">The search query string.</param>
    /// <param name="maxResults">Maximum number of results to return (default: 5).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A list of search results matching the query.</returns>
    Task<List<SearchResult>> SearchAsync(string query, int maxResults = 5, CancellationToken ct = default);
}

/// <summary>
/// Represents a single web search result.
/// </summary>
/// <param name="Title">The title of the search result.</param>
/// <param name="Url">The URL of the search result.</param>
/// <param name="Snippet">A brief text excerpt from the search result.</param>
public record SearchResult(string Title, string Url, string Snippet);
