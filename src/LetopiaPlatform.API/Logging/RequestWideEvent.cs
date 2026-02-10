namespace LetopiaPlatform.API.Logging;

/// <summary>
/// A canonical log line / wide event for a single HTTP request.
/// Built up throughout the request lifecycle, and emitted at the end of the request with all relevant info.
/// See: https://logginggsucks.com/
/// </summary>
public sealed class RequestWideEvent
{
    // Request Context
    public string RequestId { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string? QueryString { get; set; }
    public int StatusCode { get; set; }
    public long DurationMs { get; set; }
    public string? ClientIp { get; set; }
    public string? UserAgent { get; set; }

    // User context (enriched by auth middleware / controllers)
    public string? UserId { get; set; }
    public string? UserRole { get; set; }
    public string? UserEmail { get; set; }

    // Business context (enriched by controllers / services)
    public Dictionary<string, object> BusinessContext {get;} = new();

    // Error context (enriched by exception handling middleware)
    public string? ErrorType { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
    public bool? ErrorRetriable { get; set; }

    // Infrastructure context (enriched by various layers)
    public string ServiceName { get; set; } = "letopia-api";
    public string? ServiceVersion { get; set; }
    public string? Environment { get; set; }

    // Performace context (enriched by services)
    public Dictionary<string, long> PerformanceMetrics { get; } = new();
    public int DbQueryCount { get; set; }
    public int ExternalCallCount { get; set; }

    /// <summary>
    /// Add business context (e.g., community_id, post_id, action)
    /// </summary>
    public void AddBusinessContext(string key, object value)
    {
        BusinessContext[key] = value;
    }

    /// <summary>
    /// Record a performance metric (e.g., "db_query_duration_ms", "external_api_duration_ms")
    /// </summary>
    public void AddPerformanceMetric(string key, long value)
    {
        PerformanceMetrics[key] = value;
    }

    /// <summary>
    /// Convert to a dictionary for structured logging emission.
    /// </summary>
    public Dictionary<string, object?> ToDictionary()
    {
        var dict = new Dictionary<string, object?>
        {
            ["request_id"] = RequestId,
            ["method"] = Method,
            ["path"] = Path,
            ["query_string"] = QueryString,
            ["status_code"] = StatusCode,
            ["duration_ms"] = DurationMs,
            ["client_ip"] = ClientIp,
            ["user_agent"] = UserAgent,
            ["user_id"] = UserId,
            ["user_role"] = UserRole,
            ["user_email"] = UserEmail,
            ["error_type"] = ErrorType,
            ["error_message"] = ErrorMessage,
            ["error_code"] = ErrorCode,
            ["error_retriable"] = ErrorRetriable,
            ["service_name"] = ServiceName,
            ["service_version"] = ServiceVersion,
            ["environment"] = Environment,
            ["performance_metrics"] = PerformanceMetrics,
            ["db_query_count"] = DbQueryCount,
            ["external_call_count"] = ExternalCallCount
        };

        // Flatten business context into the top-level dictionary with a prefix
        foreach (var (key, value) in BusinessContext)
        {
            dict[$"biz_{key}"] = value;
        }

        foreach (var (key, value) in PerformanceMetrics)
        {
            dict[$"perf_{key}"] = value;
        }

        return dict;
    }
}