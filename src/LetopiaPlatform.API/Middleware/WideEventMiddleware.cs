using System.Diagnostics;
using System.Reflection;
using System.Security.Claims;
using LetopiaPlatform.API.Logging;
using Serilog;
using Serilog.Context;

namespace LetopiaPlatform.API.Middleware;

/// <summary>
/// Middleware that implements the "canonical log line" / wide event pattern.
/// One structured log event per request, emitted at the end.
/// See: https://logginggsucks.com/
/// </summary>
public sealed class WideEventMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly string ServiceVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";

    public WideEventMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var wideEvent = new RequestWideEvent
        {
            RequestId = context.TraceIdentifier,
            Method = context.Request.Method,
            Path = context.Request.Path.Value ?? "/",
            QueryString = context.Request.QueryString.HasValue
                ? context.Request.QueryString.Value
                : null,
            ClientIp = context.Connection.RemoteIpAddress?.ToString(),
            UserAgent = context.Request.Headers.UserAgent.ToString(),
            ServiceVersion = ServiceVersion,
            Environment = context.RequestServices.GetService<IWebHostEnvironment>()?.EnvironmentName
        };

        // Store in HttpContext.Items so controllers/services can enrich it
        context.Items["WideEvent"] = wideEvent;

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // Capture exception details in the wide event
            wideEvent.ErrorType = ex.GetType().Name;
            wideEvent.ErrorMessage = ex.Message;
            wideEvent.ErrorRetriable = false; // Could add logic to determine this based on exception type

            throw; // Re-throw so it can be handled by exception handling middleware
        }
        finally
        {
            stopwatch.Stop();
            wideEvent.DurationMs = stopwatch.ElapsedMilliseconds;
            wideEvent.StatusCode = context.Response.StatusCode;

            // Enrich user context from claims (after auth ran)
            EnrichUserContext(context, wideEvent);

            // Emit the single canonical log line
            EmitWideEvent(wideEvent);
        }
    }

    private static void EnrichUserContext(HttpContext context, RequestWideEvent wideEvent)
    {
        if (context.User.Identity?.IsAuthenticated != true)
            return;
            
        wideEvent.UserId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        wideEvent.UserEmail = context.User.FindFirst(ClaimTypes.Email)?.Value;
        wideEvent.UserRole = context.User.FindFirst(ClaimTypes.Role)?.Value;
    }

    private static void EmitWideEvent(RequestWideEvent wideEvent)
    {
        var eventData = wideEvent.ToDictionary();

        // Choose log level based on outcome
        var level = wideEvent.StatusCode switch
        {
            >= 500 => Serilog.Events.LogEventLevel.Error,
            >= 400 => Serilog.Events.LogEventLevel.Warning,
            _ => Serilog.Events.LogEventLevel.Information
        };

        // Build structured properties for Serilog
        using (LogContext.PushProperty("WideEvent", eventData, destructureObjects: true))
        {
            Log.Write(level,
                "HTTP {Method} {Path} responded {StatusCode} in {DurationMs}ms",
                wideEvent.Method,
                wideEvent.Path,
                wideEvent.StatusCode,
                wideEvent.DurationMs);
        }
    }
}