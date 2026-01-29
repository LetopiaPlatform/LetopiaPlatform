using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace LetopiaPlatform.Agent.Configuration;

public static class TracingConfiguration
{
    public const string ActivitySourceName = "LetopiaPlatform.Agent";

    public static IServiceCollection AddAgentTracing(this IServiceCollection services)
    {
        services.AddOpenTelemetry()
            .WithTracing(builder =>
            {
                builder
                    .AddSource(ActivitySourceName)
                    .SetSampler(new AlwaysOnSampler())
                    .AddConsoleExporter(); // للتجربة – لاحقًا Jaeger / AppInsights
            });

        return services;
    }
}
