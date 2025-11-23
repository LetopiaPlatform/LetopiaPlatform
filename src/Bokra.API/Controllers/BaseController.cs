using Bokra.API.Common;
using Bokra.Core.Common;
using Bokra.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Bokra.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// Returns 200 OK with data
    /// </summary>
    protected IActionResult SuccessResponse<T>(T data, string message = "Success")
    {
        return Ok(ApiResponse<T>.SuccessResponse(data, message));
    }

    /// <summary>
    /// Returns 400 Bad Request with error
    /// </summary>
    protected IActionResult BadRequestResponse(string error)
    {
        return BadRequest(ApiResponse<object>.FailureResponse(error));
    }

    /// <summary>
    /// Returns 400 Bad Request with multiple errors
    /// </summary>
    protected IActionResult BadRequestResponse(List<string> errors)
    {
        return BadRequest(ApiResponse<object>.FailureResponse(errors));
    }

    /// <summary>
    /// Handles Result pattern - return appropriate response
    /// </summary>
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return SuccessResponse(result.Value);
        }

        return BadRequestResponse(result.Errors);
    }

    /// <summary>
    /// Handles Result pattern without data
    /// </summary>
    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
        {
            return Ok(ApiResponse<object>.SuccessResponse(null, "Operation completed successfully"));
        }

        return BadRequestResponse(result.Errors);
    }

    /// <summary>
    /// Gets the current user ID from JWT claims
    /// </summary>
    protected Guid GetUserId()
    {
        var userIdClaims = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaims) || !Guid.TryParse(userIdClaims, out var userId))
        {
            throw new UnauthorizedException("User ID not found in token");
        }

        return userId;
    }

}
