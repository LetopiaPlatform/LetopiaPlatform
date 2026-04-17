using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LetopiaPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace LetopiaPlatform.Infrastructure.Data.Configurations;
public class ResourceLikeConfiguration : IEntityTypeConfiguration<ResourceLike>
{
    public void Configure(EntityTypeBuilder<ResourceLike> builder)
    {
        // ── Table & primary key ───────────────────────────────────────────────

        builder.ToTable("ResourceLikes");
        builder.HasKey(l => l.Id);

        // ── Scalar properties ─────────────────────────────────────────────────

        builder.Property(l => l.CreatedAt)
               .IsRequired();

        // ── Unique constraint: one like per user per resource ─────────────────
        // Enforced at DB level — EF will throw on duplicate inserts so
        // IsLikedByUserAsync must always be checked before AddLikeAsync.

        builder.HasIndex(l => new { l.ResourceId, l.UserId })
               .IsUnique()
               .HasDatabaseName("IX_ResourceLikes_ResourceId_UserId");

        // ── Relationships ─────────────────────────────────────────────────────

        // Like → Resource
        builder.HasOne(l => l.Resource)
               .WithMany(r => r.Likes)
               .HasForeignKey(l => l.ResourceId)
               .OnDelete(DeleteBehavior.Cascade);

        // Like → User
        builder.HasOne(l => l.User)
               .WithMany()
               .HasForeignKey(l => l.UserId)
               .OnDelete(DeleteBehavior.Restrict);   // don't delete likes when user is deleted
    }
}
