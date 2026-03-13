using LetopiaPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LetopiaPlatform.Infrastructure.Configuration;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        // ── Table & primary key ───────────────────────────────────────────────

        builder.ToTable("Tags");
        builder.HasKey(t => t.Id);

        // ── Scalar properties ─────────────────────────────────────────────────

        builder.Property(t => t.TargetType)
               .IsRequired()
               .HasConversion<int>();     // stored as 1 = Post, 2 = Resource …

        builder.Property(t => t.TargetId)
               .IsRequired();

        builder.Property(t => t.TagName)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(t => t.CreatedAt)
               .IsRequired();

        // ── Unique constraint ─────────────────────────────────────────────────
        // Prevents attaching the same tag twice to the same entity.
        // Also acts as a composite index for GetByTargetAsync queries.

        builder.HasIndex(t => new { t.TargetType, t.TargetId, t.TagName })
               .IsUnique()
               .HasDatabaseName("IX_Tags_TargetType_TargetId_TagName");

        // ── Additional index for GetByTargetAsync ─────────────────────────────
        // Covers WHERE TargetType = X AND TargetId = Y without needing
        // a full table scan when TagName is not part of the filter.

        builder.HasIndex(t => new { t.TargetType, t.TargetId })
               .HasDatabaseName("IX_Tags_TargetType_TargetId");
    }
}
