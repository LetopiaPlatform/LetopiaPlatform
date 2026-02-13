using LetopiaPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LetopiaPlatform.Infrastructure.Data.Configurations;

public class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        // Table
        builder.ToTable("groups");

        // Primary key
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Id).HasColumnName("id");

        // Properties
        builder.Property(g => g.CommunityId)
            .HasColumnName("community_id")
            .IsRequired();

        builder.Property(g => g.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(g => g.Slug)
            .HasColumnName("slug")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(g => g.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(g => g.DisplayOrder)
            .HasColumnName("display_order")
            .HasDefaultValue(0);

        builder.Property(g => g.PostCount)
            .HasColumnName("post_count")
            .HasDefaultValue(0);

        builder.Property(g => g.CreatedAt)
            .HasColumnName("created_at");

        // Indexes
        builder.HasIndex(g => new { g.CommunityId, g.Slug })
            .IsUnique()
            .HasDatabaseName("ix_groups_community_slug");

        // Relationships
        builder.HasOne(g => g.Community)
            .WithMany(c => c.Groups)
            .HasForeignKey(g => g.CommunityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(g => g.Posts)
            .WithOne(p => p.Group)
            .HasForeignKey(p => p.GroupId);
    }
}