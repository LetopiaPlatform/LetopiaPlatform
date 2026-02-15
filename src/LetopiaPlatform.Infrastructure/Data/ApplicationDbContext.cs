using System.Linq.Expressions;
using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Entities.Identity;
using LetopiaPlatform.Core.Interfaces;
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

        // Apply all IEntityTypeConfiguration<T> from this assembly
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Apply global query filter for all soft-deletable entities
        ApplySoftDeleteQueryFilter(builder);
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
        var entries = ChangeTracker.Entries<IAuditable>();

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
                    entry.Property(nameof(IAuditable.CreatedAt)).IsModified = false;
                    break;
            }
        }
    }

    private static void ApplySoftDeleteQueryFilter(ModelBuilder builder)
    {
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (!typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
                continue;
            
            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var property = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
            var filter = Expression.Lambda(Expression.Not(property), parameter);

            builder.Entity(entityType.ClrType).HasQueryFilter(filter);
        }
    }
}