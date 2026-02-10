using LetopiaPlatform.API.Logging;

namespace LetopiaPlatform.API.Extensions;

public static class WideEventExtensions
{
    /// <summary>
    /// Get the current request's wide event to enrich with business context properties.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>The wide event for the current request, or null if not available.</returns>
    public static RequestWideEvent? GetWideEvent(this HttpContext context)
    {
        return context.Items.TryGetValue("WideEvent", out var value) && value is RequestWideEvent wideEvent
            ? wideEvent
            : null;
    }

    /// <summary>
    /// Add business context to the current request's wide event.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="key">The context key.</param>
    /// <param name="value">The context value.</param>
    public static void AddBusinessContext(this HttpContext context, string key, object value)
    {
        context.GetWideEvent()?.AddBusinessContext(key, value);
    }

    /// <summary>
    /// Record performance metrics to the current request's wide event.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="operation">The name of the operation being measured.</param>
    /// <param name="milliseconds">The duration of the operation in milliseconds.</param>
    public static void RecordMetrics(this HttpContext context, string operation, long milliseconds)
    {
        context.GetWideEvent()?.AddPerformanceMetric(operation, milliseconds);
    }
}