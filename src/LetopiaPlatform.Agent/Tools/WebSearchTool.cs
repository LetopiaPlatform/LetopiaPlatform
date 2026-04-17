using System.ComponentModel;
using System.Linq;
using System.Threading;
using LetopiaPlatform.Core.Interfaces;
using Microsoft.Extensions.AI;

namespace LetopiaPlatform.Agent.Tools;

/// <summary>
/// Defines the AI tool that allows the language model to search the web
/// for learning resources using <see cref="IWebSearchService"/>.
/// </summary>
public static class WebSearchTool
{
    /// <summary>
    /// Creates the <c>search_web</c> AI function that enables the LLM
    /// to retrieve web search results such as documentation,
    /// tutorials, and learning resources.
    /// </summary>
    public static AIFunction Create(IWebSearchService searchService)
    {
        return AIFunctionFactory.Create(
            async (
                [Description("The web search query string.")]
                string query,

                [Description("Maximum number of results to return.")]
                int max_results = 5,

                CancellationToken ct = default) =>
            {
                var results = await searchService.SearchAsync(query, max_results, ct);
                return results.Select(r => new { r.Title, r.Url, r.Snippet });
            },
            name: "search_web",
            description:
                "Search the web for learning resources, documentation, courses, and articles. " +
                "Returns titles, URLs and snippets. " +
                "Use this tool to find real, current resources for each phase of a learning roadmap."
        );
    }
}
