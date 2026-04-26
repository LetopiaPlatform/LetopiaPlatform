using LetopiaPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LetopiaPlatform.Infrastructure.Data.Configurations;
public class ProjectMilestoneDetailsConfiguration : IEntityTypeConfiguration<ProjectMilestoneDetails>
{
    public void Configure(EntityTypeBuilder<ProjectMilestoneDetails> builder)
    {
        builder.ToTable("project_milestones");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id");

        builder.Property(m => m.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(m => m.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);


        builder.Property(m => m.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(m => m.ProjectId)
            .HasColumnName("project_id")
            .IsRequired();

        builder.HasOne(m => m.Project)
            .WithMany(p => p.Milestones)
            .HasForeignKey(m => m.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => m.ProjectId)
            .HasDatabaseName("ix_project_milestones_project_id");
    }
}
