using LetopiaPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LetopiaPlatform.Infrastructure.Data.Configurations;

public class CommunityConfiguration : IEntityTypeConfiguration<Community>
{
    public void Configure(EntityTypeBuilder<Community> builder)
    {
        // Table
        builder.ToTable("communities");

        // Primary key
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");

        // Audit columns
        builder.Property(c => c.CreatedAt).HasColumnName("created_at");
        builder.Property(c => c.UpdatedAt).HasColumnName("updated_at");

        // Properties
        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Slug)
            .HasColumnName("slug")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasColumnName("description")
            .IsRequired();

        builder.Property(c => c.CategoryId)
            .HasColumnName("category_id")
            .IsRequired();

        builder.Property(c => c.CreatedBy)
            .HasColumnName("created_by")
            .IsRequired();

        builder.Property(c => c.MemberCount)
            .HasColumnName("member_count")
            .HasDefaultValue(0);

        builder.Property(c => c.PostCount)
            .HasColumnName("post_count")
            .HasDefaultValue(0);

        builder.Property(c => c.CoverImageUrl)
            .HasColumnName("cover_image_url")
            .HasMaxLength(500);

        builder.Property(c => c.IsPrivate)
            .HasColumnName("is_private")
            .HasDefaultValue(false);

        builder.Property(c => c.LastPostAt)
            .HasColumnName("last_post_at");

        builder.Property(c => c.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(c => c.Rules)
            .HasColumnName("rules")
            .HasConversion(
                v => string.Join("||", v), // Convert List<string> to a single string for storage
                v => v.Split("||", StringSplitOptions.RemoveEmptyEntries).ToList()) // Convert stored string back to List<string>
            .HasColumnType("text"); // Store as text in the database

        // Indexes
        builder.HasIndex(c => c.Slug)
            .IsUnique()
            .HasDatabaseName("ix_communities_slug");

        builder.HasIndex(c => c.CategoryId)
            .HasDatabaseName("ix_communities_category_id");

        builder.HasIndex(c => c.CreatedBy)
            .HasDatabaseName("ix_communities_created_by");

        // Relationships
        builder.HasOne(c => c.CreatedByUser)
            .WithMany()
            .HasForeignKey(c => c.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Posts)
            .WithOne(p => p.Community)
            .HasForeignKey(p => p.CommunityId);

        builder.HasMany(c => c.Members)
            .WithOne(uc => uc.Community)
            .HasForeignKey(uc => uc.CommunityId);

        builder.HasMany(c => c.Channels)
            .WithOne(ch => ch.Community)
            .HasForeignKey(ch => ch.CommunityId);
    }
}