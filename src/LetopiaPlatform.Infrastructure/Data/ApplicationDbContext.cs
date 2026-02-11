using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LetopiaPlatform.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<User, Role, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {


    }

    public DbSet<Community> Communities => Set<Community>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<UserCommunity> UserCommunities => Set<UserCommunity>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>(entity => { entity.ToTable("AspNetUsers"); });
        builder.Entity<Role>(entity => { entity.ToTable("AspNetRoles"); });
        builder.Entity<IdentityUserRole<Guid>>(entity => { entity.ToTable("AspNetUserRoles"); });
        builder.Entity<IdentityUserClaim<Guid>>(entity => { entity.ToTable("AspNetUserClaims"); });
        builder.Entity<IdentityUserLogin<Guid>>(entity => { entity.ToTable("AspNetUserLogins"); });
        builder.Entity<IdentityRoleClaim<Guid>>(entity => { entity.ToTable("AspNetRoleClaims"); });
        builder.Entity<IdentityUserToken<Guid>>(entity => { entity.ToTable("AspNetUserTokens"); });
    }


}
