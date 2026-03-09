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
        context.Request.EnableBuffering();
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
                _logger.LogWarning("Validation failed: {Message}", ve.Message);
                break;

            case UnauthorizedException ue:
                statusCode = StatusCodes.Status401Unauthorized;
                message = ue.Message;
                _logger.LogWarning("Unauthorized access: {Message}", ue.Message);
                break;

            case ForbiddenException fe:
                statusCode = StatusCodes.Status403Forbidden;
                message = fe.Message;
                _logger.LogWarning("Forbidden access: {Message}", fe.Message);
                break;

            case NotFoundException ne:
                statusCode = StatusCodes.Status404NotFound;
                message = ne.Message;
                _logger.LogInformation("Resource not found: {Message}", ne.Message);
                break;

            case ConflictException ce:
                statusCode = StatusCodes.Status409Conflict;
                message = ce.Message;
                _logger.LogWarning("Conflict occurred: {Message}", ce.Message);
                break;

            case AppException ae:
                statusCode = ae.StatusCode;
                message = ae.Message;
                _logger.LogWarning(ae, "Application error: ({StatusCode}) {Message}", ae.StatusCode, ae.Message);
                break;

            default:
                statusCode = StatusCodes.Status500InternalServerError;
                message = "An internal server error occurred.";

                var rootCause = GetInnermostException(exception);
                var requestBody = await ReadRequestBodyAsync(context);

                _logger.LogError(exception,
                    "Unhandled exception on: {Method} {Path}{QueryString} | " +
                    "ExceptionType: {ExceptionType} | Message: {ExceptionMessage} | " +
                    "InnerException: {InnerException} | RootCause: {RootCauseType}: {RootCauseMessage} | " +
                    "RequestBody: {RequestBody} | " +
                    "User: {UserId} | TraceId: {TraceId}",
                    context.Request.Method,
                    context.Request.Path,
                    Sanitize(context.Request.QueryString.ToString()),
                    exception.GetType().FullName,
                    Sanitize(exception.Message),
                    Sanitize(exception.InnerException?.Message ?? "N/A"),
                    rootCause.GetType().FullName,
                    Sanitize(rootCause.Message),
                    Sanitize(requestBody),
                    context.User?.FindFirst("sub")?.Value ?? "anonymous",
                    context.TraceIdentifier);
                break;
        }

        var response = new ErrorResponse
        {
            Status = statusCode,
            Message = _environment.IsDevelopment() && statusCode >= StatusCodes.Status500InternalServerError ? exception.Message : message,
            Errors = errors
        };

        // In development, add stack trace for 5xxs only
        if (_environment.IsDevelopment() && statusCode >= StatusCodes.Status500InternalServerError)
        {
            response.Errors.Add(exception.StackTrace ?? string.Empty);
        }

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }

    private static async Task<string> ReadRequestBodyAsync(HttpContext context)
    {
        try
        {
            context.Request.Body.Position = 0;
            using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            return string.IsNullOrWhiteSpace(body) ? "N/A" : body.Length > 4096 ? body[..4096] + "...(truncated)" : body;
        }
        catch
        {
            return "N/A";
        }
    }

    private static Exception GetInnermostException(Exception exception)
    {
        while (exception.InnerException != null)
            exception = exception.InnerException;
        return exception;
    }

    /// <summary>
    /// Strips newlines and control characters to prevent log injection / log forging.
    /// </summary>
    private static string Sanitize(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return value
            .Replace("\r", "", StringComparison.Ordinal)
            .Replace("\n", "", StringComparison.Ordinal)
            .Replace("\t", " ", StringComparison.Ordinal);
    }
}