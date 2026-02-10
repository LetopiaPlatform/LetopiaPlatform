using LetopiaPlatform.API.Middleware;
using LetopiaPlatform.API.Validators;
using LetopiaPlatform.Core.Entities.Identity;
using LetopiaPlatform.Infrastructure;
using LetopiaPlatform.Infrastructure.Seeder;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using FluentValidation.AspNetCore;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers()
        ;
        builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
        builder.Services.AddAgentServices(builder.Configuration);
        builder.Services.AddHttpContextAccessor();

        /// Fluent Validation: register validators + wire into MVC pipeline
        builder.Services.AddFluentValidationAutoValidation();
        builder.Services.AddValidatorsFromAssemblyContaining<LoginDtoValidator>();

        var app = builder.Build();

        // Seed data
        using (var scope = app.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
            await RoleSeeder.SeedAsync(roleManager);
            await UserSeeder.SeedAsync(userManager);
        }
        
        app.UseMiddleware<ExceptionMiddleware>();
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}
