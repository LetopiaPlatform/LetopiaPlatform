using LetopiaPlatform.Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace LetopiaPlatform.Infrastructure.Seeder
{
    public static class UserSeeder
    {
        public static async Task SeedAsync(UserManager<User> userManager, IConfiguration configuration)
        {
            if (await userManager.Users.AnyAsync())
                return;
            
            var adminEmail = configuration["SeedData:AdminEmail"]
                ?? throw new InvalidOperationException("SeedData:AdminEmail configuration is missing.");
            var adminPassword = configuration["SeedData:AdminPassword"]
                ?? throw new InvalidOperationException("SeedData:AdminPassword configuration is missing.");
            
            var admin = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                Role = "Admin",
                PhoneNumberConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(admin, adminPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create admin user: {errors}");
            }

            await userManager.AddToRoleAsync(admin, "Admin");
        }

    }
}
