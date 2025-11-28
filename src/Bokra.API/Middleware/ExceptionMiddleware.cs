using Bokra.Core.Exceptions;
using System.Text.Json;

namespace Bokra.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Log the exception
        _logger.LogError(exception, $"An error occurred: {exception.Message}");

        // Set response content type
        context.Response.ContentType = "application/json";

        // Determine status code and error response
        var (statusCode, errorResponse) = exception switch
        {
            ValidationException validationEx => (
                StatusCodes.Status400BadRequest,
                new ErrorResponse
                {
                    Status = StatusCodes.Status400BadRequest,
                    Message = validationEx.Message,
                    Errors = validationEx.ValidationErrors
                }
            ),
            UnauthorizedException unauthorizedEx => (
                StatusCodes.Status401Unauthorized,
                new ErrorResponse
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Message = unauthorizedEx.Message
                }
            ),
            ForbiddenException forbiddenEx => (
                StatusCodes.Status403Forbidden,
                new ErrorResponse
                {
                    Status = StatusCodes.Status403Forbidden,
                    Message = forbiddenEx.Message
                }
            ),
            NotFoundException notFoundEx => (
                StatusCodes.Status404NotFound,
                new ErrorResponse
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = notFoundEx.Message
                }
            ),
            ConflictException conflictEx => (
                StatusCodes.Status409Conflict,
                new ErrorResponse
                {
                    Status = StatusCodes.Status409Conflict,
                    Message = conflictEx.Message
                }
            ),
            AppException appEx => (
                appEx.StatusCode,
                new ErrorResponse
                {
                    Status = appEx.StatusCode,
                    Message = appEx.Message
                }
            ),
            _ => (
                StatusCodes.Status500InternalServerError,
                new ErrorResponse
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Message = _environment.IsDevelopment()
                        ? exception.Message
                        : "An internal server error occured",
                    Details = _environment.IsDevelopment()
                        ? exception.StackTrace
                        : null
                }
            )

        };

        context.Response.StatusCode = statusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(errorResponse, options);
        await context.Response.WriteAsync(json);
    }
}

/// <summary>
/// Standard error response format
/// </summary>
public class ErrorResponse
{
    public int Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string>? Errors { get; set; }
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}