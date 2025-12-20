using LetopiaPlatform.API.Common;
using LetopiaPlatform.API.Middleware;
using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LetopiaPlatform.API.Controllers;

[Route("api/[controller]")]
[ApiController]

public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// Handles Result pattern for generic responses
    /// </summary>
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return SuccessResponse(result.Value!, "Success", result.StatusCode);

        return FailureResponse(result);
    }

    /// <summary>
    /// Handles Result pattern without data
    /// </summary>
    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
            return SuccessResponse<object>(null, "Success", result.StatusCode);

        return StatusCode(result.StatusCode, new ErrorResponse
        {
            Status = result.StatusCode,
            Message = result.Error,
            Errors = result.Errors
        });
    }

    /// <summary>
    /// Gets the current user ID from JWT claims
    /// </summary>
    protected Guid GetUserId()
    {
        var userIdClaims = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaims) || !Guid.TryParse(userIdClaims, out var userId))
            throw new UnauthorizedException("User ID not found in token");

        return userId;
    }

    /// <summary>
    /// Returns a success response with customizable status code
    /// </summary>
    private IActionResult SuccessResponse<T>(T data, string message = "Success", int statusCode = 200)
    {
        var response = ApiResponse<T>.SuccessResponse(data, message, statusCode);
        return StatusCode(statusCode, response);
    }

    /// <summary>
    /// Returns a failure response with multiple errors
    /// </summary>
    private IActionResult FailureResponse<T>(Result<T> result)
    {
        var response = new ErrorResponse
        {
            Status = result.StatusCode,
            Message = result.Error,
            Errors = result.Errors
        };

        return StatusCode(result.StatusCode, response);
    }

}
