using LetopiaPlatform.Core.Common;
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

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyAuditTimestamps();
        return base.SaveChanges();
    }

    private void ApplyAuditTimestamps()
    {
        var entries = ChangeTracker.Entries<AuditableEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    // Prevent overwriting CreatedAt on updates
                    entry.Property(nameof(AuditableEntity.CreatedAt)).IsModified = false;
                    break;
            }
        }
    }
}