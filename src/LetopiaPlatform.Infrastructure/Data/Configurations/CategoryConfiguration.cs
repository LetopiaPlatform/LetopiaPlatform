using LetopiaPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LetopiaPlatform.Infrastructure.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");

        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Slug)
            .HasColumnName("slug")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.IconUrl)
            .HasColumnName("icon_url")
            .HasMaxLength(500);

        builder.Property(c => c.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // Unique slug per type
        builder.HasIndex(c => new { c.Slug, c.Type })
            .IsUnique()
            .HasDatabaseName("ix_categories_slug_type");

        builder.HasIndex(c => c.Type)
            .HasDatabaseName("ix_categories_type");

        // Relationships
        builder.HasMany(c => c.Communities)
            .WithOne(cm => cm.Category)
            .HasForeignKey(cm => cm.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}