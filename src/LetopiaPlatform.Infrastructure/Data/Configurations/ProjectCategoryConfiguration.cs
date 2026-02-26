using LetopiaPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LetopiaPlatform.Infrastructure.Data.Configurations;
public class ProjectCategoryConfiguration : IEntityTypeConfiguration<ProjectCategory>
{
    public void Configure(EntityTypeBuilder<ProjectCategory> builder)
    {
        // Table
        builder.ToTable("project_categories");

        // Primary key
        builder.HasKey(pc => pc.Id);
        builder.Property(pc => pc.Id).HasColumnName("id");

        // Properties
        builder.Property(pc => pc.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(pc => pc.Slug)
            .HasColumnName("slug")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(pc => pc.IconUrl)
            .HasColumnName("icon_url");

        builder.Property(pc => pc.DisplayOrder)
            .HasColumnName("display_order")
            .HasDefaultValue(0);

        // Indexes
        builder.HasIndex(pc => pc.Slug)
            .IsUnique()
            .HasDatabaseName("ix_project_categories_slug");

        // Relationships (Defined on this side for clarity)
        builder.HasMany(pc => pc.Projects)
            .WithOne(p => p.Category)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
