using LetopiaPlatform.Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace LetopiaPlatform.Infrastructure.Seeder;

public static class RoleSeeder
{
    public static async Task SeedAsync(RoleManager<Role> roleManager)
    {
        var rolecount = roleManager.Roles.Count();
        if (rolecount <= 0)
        {
            await roleManager.CreateAsync(new Role() { Name = "Learner" });
            await roleManager.CreateAsync(new Role() { Name = "Guide" });
            await roleManager.CreateAsync(new Role() { Name = "Architect" });
            await roleManager.CreateAsync(new Role() { Name = "Admin" });
        }
        //Learner / Guide / Architect
    }
}
