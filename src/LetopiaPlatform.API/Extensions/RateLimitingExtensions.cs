using System.Security.Claims;
using System.Threading.RateLimiting;

namespace LetopiaPlatform.API.Extensions;

/// <summary>
/// Configures ASP.NET Core rate limiting policies.
/// </summary>
public static class RateLimitingExtensions
{
    public const string GlobalPolicy = "global";
    public const string AiGenerationPolicy = "ai-generation";
    public const string AiChatPolicy = "ai-chat";
    public const string FileUploadPolicy = "file-upload";
    public const string AuthPolicy = "auth";

    public static IServiceCollection AddRateLimitingPolicies(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // Return 429 with Retry-After header.
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
                }

                context.HttpContext.Response.ContentType = "application/json";
                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    status = 429,
                    error = "Too many requests. Please try again later.",
                    retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retry)
                        ? (int)retry.TotalSeconds
                        : (int?)null
                }, cancellationToken);
            };

            // Global policy: 100 req/min per user or IP
            options.AddPolicy(GlobalPolicy, context =>
            {
                var partitionKey = GetPartitionKey(context);

                return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true
                });
            });

            // AI roadmap generation: 5 req/day per user
            options.AddPolicy(AiGenerationPolicy, context =>
            {
                var partitionKey = GetPartitionKey(context);

                return RateLimitPartition.GetFixedWindowLimiter($"ai-gen:{partitionKey}", _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromDays(1),
                    QueueLimit = 0,
                    AutoReplenishment = true
                });
            });

            // AI chat: 30 req/hour per user
            options.AddPolicy(AiChatPolicy, context =>
            {
                var partitionKey = GetPartitionKey(context);

                return RateLimitPartition.GetFixedWindowLimiter($"ai-chat:{partitionKey}", _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 30,
                    Window = TimeSpan.FromHours(1),
                    QueueLimit = 0,
                    AutoReplenishment = true
                });
            });

            // File uploads: 20 req/day per user
            options.AddPolicy(FileUploadPolicy, context =>
            {
                var partitionKey = GetPartitionKey(context);

                return RateLimitPartition.GetFixedWindowLimiter($"file-upload:{partitionKey}", _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 20,
                    Window = TimeSpan.FromHours(1),
                    QueueLimit = 0,
                    AutoReplenishment = true
                });
            });

            // Auth endpoints: 10 req/min per IP (brute-force protection)
            options.AddPolicy(AuthPolicy, context =>
            {
                var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter($"auth:{ipAddress}", _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true
                });
            });
        });

        return services;
    }

    /// <summary>
    /// Returns the authenticated user ID if available, otherwise the client IP address.
    /// </summary>
    private static string GetPartitionKey(HttpContext context)
    {
        var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            return userId;
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}