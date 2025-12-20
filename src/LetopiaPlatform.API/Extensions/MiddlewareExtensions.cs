using LetopiaPlatform.API.Middleware;

namespace LetopiaPlatform.API.Extensions;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHanlder(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionMiddleware>();
        return app;
    }

    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        // TODO: Request logging middleware implemented here.
        return app;
    }
}