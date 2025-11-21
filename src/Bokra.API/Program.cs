using Bokra.Core.Entites.Identity;
using Bokra.Infrastructure.Data.Configuration;
using Bokra.Infrastructure.Seeder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        #region AddContext
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
        #endregion

        #region AddIdentity
        builder.Services.AddIdentity<User, Role>()
                     .AddEntityFrameworkStores<ApplicationDbContext>()
                     .AddDefaultTokenProviders();
        #endregion



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

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseAuthentication();
        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
