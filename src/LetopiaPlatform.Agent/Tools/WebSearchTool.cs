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
                [Description("The web search query string. Must be a specific, non-empty search query like 'best beginner backend development course official'.")]
                string query,

                CancellationToken ct = default) =>
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return (object)("Error: The search query was empty. You must provide a specific search query string. " +
                           "Example: 'best beginner backend development course official'. " +
                           "Please call search_web again with a proper query.");
                }

                var results = await searchService.SearchAsync(query, 0, ct);
                return (object)results.Select(r => new { r.Title, r.Url, r.Snippet });
            },
            name: "search_web",
            description:
                "Search the web for learning resources, documentation, courses, and articles. " +
                "Returns titles, URLs and snippets. " +
                "Use this tool to find real, current resources for each phase of a learning roadmap. " +
                "IMPORTANT: Always provide a specific, non-empty search query."
        );
    }
}
