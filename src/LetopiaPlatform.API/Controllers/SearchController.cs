using LetopiaPlatform.API.AppMetaData;
using LetopiaPlatform.API.Common;
using LetopiaPlatform.API.Extensions;
using LetopiaPlatform.Core.DTOs.Search;
using LetopiaPlatform.Core.Exceptions;
using LetopiaPlatform.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LetopiaPlatform.API.Controllers;

[ApiController]
public class SearchController : BaseController
{
    private readonly ISearchService _searchService;

    public SearchController(ISearchService searchService)
    {
        _searchService = searchService;
    }

    [HttpGet(Router.Search.Query)]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<GlobalSearchResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Search(
        [FromQuery] string query,
        [FromQuery] string? type,
        [FromQuery] int limit = 5,
        CancellationToken ct = default)
    {
        HttpContext.AddBusinessContext("action", "global_search");

        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
        {
            throw new AppException("Search query must be at least 2 characters.", 400);
        }

        if (limit is < 1 or > 10)
        {
            throw new AppException("Limit must be between 1 and 10.", 400);
        }

        string[] validTypes = ["communities", "projects", "members"];
        if (type is not null && !validTypes.Contains(type.ToLowerInvariant()))
        {
            throw new AppException($"Type must be one of: {string.Join(", ", validTypes)}.", 400);
        }

        var results = await _searchService.SearchAsync(query, type?.ToLowerInvariant(), limit, ct);

        return Ok(ApiResponse<GlobalSearchResultDto>.SuccessResponse(results));

    }
}
