using LetopiaPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LetopiaPlatform.Infrastructure.Data.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        // 1. Table Name & Primary Key
        builder.ToTable("projects");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");

        // 2. Basic Properties
        builder.Property(p => p.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasColumnName("description");



        // 3. Mapping Lists to Postgres Arrays (Skills & Goals)
        builder.Property(p => p.RequiredSkills)
            .HasColumnName("required_skills")
            .HasColumnType("text[]");



        // 4. Enums & Booleans
        builder.Property(p => p.DifficultyLevel)
            .HasColumnName("difficulty_level")
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(p => p.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(p => p.IsPublic)
            .HasColumnName("is_public")
            .HasDefaultValue(true);



        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(p => p.OwnerId).HasColumnName("owner_id");
        builder.Property(p => p.CategoryId).HasColumnName("category_id");

        // 7. Relationships

        builder.HasOne(p => p.Owner)
            .WithMany(u => u.OwnedProjects)
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Category)
            .WithMany(pc => pc.Projects)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);


        builder.HasMany(p => p.Milestones)
            .WithOne(m => m.Project)
            .HasForeignKey(m => m.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Resources)
            .WithOne(r => r.Project)
            .HasForeignKey(r => r.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // 8. Indexes
        builder.HasIndex(p => p.OwnerId).HasDatabaseName("ix_projects_owner_id");
        builder.HasIndex(p => p.CategoryId).HasDatabaseName("ix_projects_category_id");
    }
}
