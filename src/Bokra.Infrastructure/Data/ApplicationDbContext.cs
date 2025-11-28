using Bokra.Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
//using StackExchange.Redis;

namespace Bokra.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, Role, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {


        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>(entity => { entity.ToTable("AspNetUsers"); });
            builder.Entity<Role>(entity => { entity.ToTable("AspNetRoles"); });
            builder.Entity<IdentityUserRole<int>>(entity => { entity.ToTable("AspNetUserRoles"); });
            builder.Entity<IdentityUserClaim<int>>(entity => { entity.ToTable("AspNetUserClaims"); });
            builder.Entity<IdentityUserLogin<int>>(entity => { entity.ToTable("AspNetUserLogins"); });
            builder.Entity<IdentityRoleClaim<int>>(entity => { entity.ToTable("AspNetRoleClaims"); });
            builder.Entity<IdentityUserToken<int>>(entity => { entity.ToTable("AspNetUserTokens"); });
        }

    }
}
