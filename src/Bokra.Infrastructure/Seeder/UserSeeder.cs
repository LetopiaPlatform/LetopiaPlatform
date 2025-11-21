using Bokra.Core.Entites.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Bokra.Infrastructure.Seeder
{
    public static class UserSeeder
    {
        public static async Task SeedAsync(UserManager<User> _usermanager)
        {
            var usercount = await _usermanager.Users.CountAsync();
            if (usercount <= 0)
            {
                var defaultuser = new User()
                {
                    UserName = "admin",
                    Email = "admin@project.com",
                    PhoneNumber = "123456",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                };
                await _usermanager.CreateAsync(defaultuser, "Admin@123");
                await _usermanager.AddToRoleAsync(defaultuser, "Admin");

            }

        }

    }
}
