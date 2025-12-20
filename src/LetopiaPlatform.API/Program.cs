using LetopiaPlatform.API.Middleware;
using LetopiaPlatform.API.Validators;
using LetopiaPlatform.Core.Entities.Identity;
using LetopiaPlatform.Infrastructure;
using LetopiaPlatform.Infrastructure.Data;
using LetopiaPlatform.Infrastructure.Seeder;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers()
        ;
        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddHttpContextAccessor();

        /// reigister fluentValidations
        builder.Services.AddValidatorsFromAssemblyContaining<LoginDtoValidator>();
        var app = builder.Build();

        // Configure the HTTP request pipeline.

        #region DataSeeding
        using (var scope = app.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
            await RoleSeeder.SeedAsync(roleManager);
            await UserSeeder.SeedAsync(userManager);
        }
        #endregion
        app.UseMiddleware<ExceptionMiddleware>();
        app.UseRouting();
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseAuthentication();
        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
