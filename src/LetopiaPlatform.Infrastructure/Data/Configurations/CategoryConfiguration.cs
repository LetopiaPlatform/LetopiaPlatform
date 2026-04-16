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

        builder.Property(c => c.ParentCategoryId)
            .HasColumnName("parent_category_id")
            .IsRequired(false);
        
        // Unique slug per type
        builder.HasIndex(c => new { c.Slug, c.Type })
            .IsUnique()
            .HasDatabaseName("ix_categories_slug_type");

        // Unique name per hierarchy level (same parent + type)
        // AreNullsDistinct(false) ensures two root categories with NULL parent are treated as duplicates
        builder.HasIndex(c => new { c.Name, c.Type, c.ParentCategoryId })
            .IsUnique()
            .AreNullsDistinct(false)
            .HasDatabaseName("ix_categories_name_type_parent");

        builder.HasIndex(c => c.Type)
            .HasDatabaseName("ix_categories_type");

        builder.HasIndex(c => c.ParentCategoryId)
            .HasDatabaseName("ix_categories_parent_category_id");
            
        // Relationships
        builder.HasMany(c => c.Communities)
            .WithOne(cm => cm.Category)
            .HasForeignKey(cm => cm.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.ParentCategory)
            .WithMany(c => c.ChildCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}