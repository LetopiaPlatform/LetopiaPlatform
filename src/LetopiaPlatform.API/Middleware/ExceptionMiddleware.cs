using LetopiaPlatform.API.Common;
using LetopiaPlatform.Core.Exceptions;
using System.Text.Json;

namespace LetopiaPlatform.API.Middleware;

public class ExceptionMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IWebHostEnvironment environment)
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
        _logger.LogError(exception, "Unhandled exception occurred");

        context.Response.ContentType = "application/json";

        int statusCode;
        List<string> errors = new();
        string message;

        switch (exception)
        {
            case ValidationException ve:
                statusCode = StatusCodes.Status400BadRequest;
                errors.AddRange(ve.ValidationErrors);
                message = ve.Message;
                break;

            case UnauthorizedException ue:
                statusCode = StatusCodes.Status401Unauthorized;
                message = ue.Message;
                break;

            case ForbiddenException fe:
                statusCode = StatusCodes.Status403Forbidden;
                message = fe.Message;
                break;

            case NotFoundException ne:
                statusCode = StatusCodes.Status404NotFound;
                message = ne.Message;
                break;

            case ConflictException ce:
                statusCode = StatusCodes.Status409Conflict;
                message = ce.Message;
                break;

            case AppException ae:
                statusCode = ae.StatusCode;
                message = ae.Message;
                break;

            default:
                statusCode = StatusCodes.Status500InternalServerError;
                message = _environment.IsDevelopment() ? exception.Message : "An internal server error occurred";
                errors.Add(_environment.IsDevelopment() ? exception.StackTrace ?? string.Empty : string.Empty);
                break;
        }

        var response = new ErrorResponse
        {
            Status = statusCode,
            Message = message,
            Errors = errors
        };

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}