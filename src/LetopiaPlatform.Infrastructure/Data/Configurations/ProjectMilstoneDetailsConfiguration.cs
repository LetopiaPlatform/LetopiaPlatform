using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LetopiaPlatform.Infrastructure.Data.Configurations;

public class ProjectMilestoneDetailsConfiguration : IEntityTypeConfiguration<ProjectMilestoneDetails>
{
    public void Configure(EntityTypeBuilder<ProjectMilestoneDetails> builder)
    {
        // 1. Table Name & Primary Key
        builder.ToTable("project_milestones");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // 2. Properties
        builder.Property(m => m.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(m => m.Description)
            .HasColumnName("description")
            .HasMaxLength(500)
            .IsRequired(false);
        builder.Property(m => m.DurationText)
            .HasColumnName("duration_text")
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(m => m.Status)
            .HasColumnName("status")
            .HasConversion(
                v => v.ToString(),
                v => (MilestoneStatus)Enum.Parse(typeof(MilestoneStatus), v))
            .HasMaxLength(30)
            .HasDefaultValue(MilestoneStatus.Pending);

        // 3. Foreign Key & Auditing
        builder.Property(m => m.ProjectId)
            .HasColumnName("project_id")
            .IsRequired();

        builder.Property(m => m.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(m => m.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired(true);

        builder.HasOne(m => m.Project)
            .WithMany(p => p.Milestones)
            .HasForeignKey(m => m.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // 5. Indexes
        builder.HasIndex(m => m.ProjectId)
            .HasDatabaseName("ix_milestones_project_id");
    }
}
