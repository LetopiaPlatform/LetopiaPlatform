using LetopiaPlatform.Agent;
using LetopiaPlatform.API.Extensions;
using LetopiaPlatform.API.Middleware;
using LetopiaPlatform.Core.Entities.Identity;
using LetopiaPlatform.Infrastructure;
using LetopiaPlatform.Infrastructure.Seeder;
using Microsoft.AspNetCore.Identity;
using Serilog;

namespace LetopiaPlatform.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        try
        {
            Log.Information("Starting Letopia Platform API");

            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services));

            // ── Service registration ──────────────────────────────────────
            builder.Services
                .AddApiServices(builder.Configuration)
                .AddInfrastructure(builder.Configuration, builder.Environment)
                .AddAgentServices(builder.Configuration);

            var app = builder.Build();

            // ── Seed data ─────────────────────────────────────────────────
            await SeedDataAsync(app);

            // ── Middleware pipeline — order matters ───────────────────────
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Letopia Platform API v1");
                    options.DisplayRequestDuration();
                });
            }

            app.UseSerilogRequestLogging();
            app.UseMiddleware<ExceptionMiddleware>();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseCors("DefaultPolicy");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapHealthChecks("/health");
            app.MapControllers();
            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    private static async Task SeedDataAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        await RoleSeeder.SeedAsync(services.GetRequiredService<RoleManager<Role>>());
        await UserSeeder.SeedAsync(
            services.GetRequiredService<UserManager<User>>(),
            services.GetRequiredService<IConfiguration>());
    }
}
