using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LetopiaPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace LetopiaPlatform.Infrastructure.Data.Configurations;
public class CommunityResourceConfiguration : IEntityTypeConfiguration<CommunityResource>
{
    public void Configure(EntityTypeBuilder<CommunityResource> builder)
    {
        // ── Table & primary key ───────────────────────────────────────────────

        builder.ToTable("CommunityResources");
        builder.HasKey(r => r.Id);

        // ── Scalar properties ─────────────────────────────────────────────────

        builder.Property(r => r.Title)
               .IsRequired()
               .HasMaxLength(300);

        builder.Property(r => r.Url)
               .IsRequired()
               .HasMaxLength(2048);

        builder.Property(r => r.Description)
               .HasMaxLength(1000);

        builder.Property(r => r.ThumbnailUrl)
               .HasMaxLength(2048);

        builder.Property(r => r.Type)
               .IsRequired()
               .HasConversion<int>();        // store as int, not string

        builder.Property(r => r.ViewsCount)
               .HasDefaultValue(0);

        builder.Property(r => r.LikesCount)
               .HasDefaultValue(0);

        builder.Property(r => r.IsDeleted)
               .HasDefaultValue(false);

        builder.Property(r => r.CreatedAt)
               .IsRequired();

        // ── Soft-delete global query filter ───────────────────────────────────
        // Every LINQ query on DbSet<Resource> automatically appends
        // WHERE IsDeleted = 0 — no need to remember it anywhere else.

        builder.HasQueryFilter(r => !r.IsDeleted);

        // ── Relationships ─────────────────────────────────────────────────────

        // Resource → Community (many-to-one)
        // Deleting a community cascades and removes all its resources.
        builder.HasOne(r => r.Community)
               .WithMany()
               .HasForeignKey(r => r.CommunityId)
               .OnDelete(DeleteBehavior.Cascade);

        // Resource → User (uploader, many-to-one)
        // Restrict prevents accidental user deletion when they have resources.
        builder.HasOne(r => r.UploadedBy)
               .WithMany()
               .HasForeignKey(r => r.CreatedBy)
               .OnDelete(DeleteBehavior.Restrict);

        // Resource → ResourceLike (one-to-many)
        builder.HasMany(r => r.Likes)
               .WithOne(l => l.Resource)
               .HasForeignKey(l => l.ResourceId)
               .OnDelete(DeleteBehavior.Cascade);


    }
}
