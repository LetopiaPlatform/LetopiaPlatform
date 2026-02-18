using LetopiaPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LetopiaPlatform.Infrastructure.Data.Configurations;
public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        // 1. Table & Primary Key
        builder.ToTable("projects");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");

        // 2. Basic Properties (Snake Case Mapping)
        builder.Property(p => p.Title).HasColumnName("title").IsRequired();
        builder.Property(p => p.Description).HasColumnName("description").IsRequired();
        builder.Property(p => p.DifficultyLevel).HasColumnName("difficulty_level");
        builder.Property(p => p.Deadline).HasColumnName("deadline").IsRequired();
        builder.Property(p => p.IsFull).HasColumnName("is_full").HasDefaultValue(false);

        builder.Property(p => p.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .HasDefaultValue("Recruiting");

        builder.Property(p => p.MaxMembers)
            .HasColumnName("max_members")
            .HasDefaultValue(5);

        builder.Property(p => p.RequiredSkills)
            .HasColumnName("required_skills"); // PostgreSQL text[]

        builder.Property(p => p.CoverImageUrl)
            .HasColumnName("cover_image_url");

        // 3. Audit Properties (From AuditableEntity)
        builder.Property(p => p.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at").IsRequired();

        // 4. Relationships & Foreign Keys
        builder.Property(p => p.OwnerId).HasColumnName("owner_id");
        builder.Property(p => p.CategoryId).HasColumnName("category_id");

        // Owner Relationship
        builder.HasOne(p => p.Owner)
            .WithMany(u => u.OwnedProjects)
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Category Relationship
        builder.HasOne(p => p.Category)
            .WithMany(pc => pc.Projects)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // 5. Indexes for Performance
        builder.HasIndex(p => p.OwnerId).HasDatabaseName("ix_projects_owner_id");
        builder.HasIndex(p => p.CategoryId).HasDatabaseName("ix_projects_category_id");
    }
}
