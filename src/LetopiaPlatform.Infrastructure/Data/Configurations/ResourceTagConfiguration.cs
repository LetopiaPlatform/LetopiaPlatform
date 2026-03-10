using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LetopiaPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace LetopiaPlatform.Infrastructure.Data.Configurations;
public class ResourceTagConfiguration : IEntityTypeConfiguration<ResourceTag>
{
    public void Configure(EntityTypeBuilder<ResourceTag> builder)
    {
        // ── Table & primary key ───────────────────────────────────────────────

        builder.ToTable("ResourceTags");
        builder.HasKey(t => t.Id);

        // ── Scalar properties ─────────────────────────────────────────────────

        builder.Property(t => t.TagName)
               .IsRequired()
               .HasMaxLength(100);

        // ── Unique constraint: no duplicate tags on the same resource ─────────

        builder.HasIndex(t => new { t.ResourceId, t.TagName })
               .IsUnique()
               .HasDatabaseName("IX_ResourceTags_ResourceId_TagName");

        // ── Relationship ──────────────────────────────────────────────────────

        builder.HasOne(t => t.Resource)
               .WithMany(r => r.Tags)
               .HasForeignKey(t => t.ResourceId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
