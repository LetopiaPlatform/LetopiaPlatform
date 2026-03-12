using LetopiaPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LetopiaPlatform.Infrastructure.Data.Configurations;
public class CommunityTaskCategoryConfiguration : IEntityTypeConfiguration<CommunityTaskCategory>
{
    public void Configure(EntityTypeBuilder<CommunityTaskCategory> builder)
    {
        // 1. Table & Primary Key
        builder.ToTable("community_task_categories");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");

        // 2. Basic Properties (Snake Case Mapping)
        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.ColorHex)
            .HasColumnName("color_hex")
            .HasMaxLength(10)
            .HasDefaultValue("#6366f1")
            .IsRequired();

        builder.Property(c => c.IconKey)
            .HasColumnName("icon_key")
            .HasMaxLength(50);

        // 3. Audit Properties (From AuditableEntity)
        builder.Property(c => c.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(c => c.UpdatedAt).HasColumnName("updated_at").IsRequired();

        // 4. Relationships & Foreign Keys
        builder.Property(c => c.CommunityId).HasColumnName("community_id");

        // Community Relationship
        builder.HasOne(c => c.Community)
            .WithMany(com => com.TaskCategories)
            .HasForeignKey(c => c.CommunityId)
            .OnDelete(DeleteBehavior.Cascade);

        // Tasks Relationship (One-to-Many)
        builder.HasMany(c => c.Tasks)
            .WithOne(t => t.Category)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        // 5. Indexes for Performance
        builder.HasIndex(c => c.CommunityId).HasDatabaseName("ix_community_task_categories_community_id");
    }
}
