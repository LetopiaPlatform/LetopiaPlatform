using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LetopiaPlatform.Infrastructure.Data.Configurations;
public class CommunityTaskConfiguration : IEntityTypeConfiguration<CommunityTask>
{
    public void Configure(EntityTypeBuilder<CommunityTask> builder)
    {
        // 1. Table & Primary Key
        builder.ToTable("community_tasks");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id");

        // 2. Basic Properties
        builder.Property(t => t.Title).HasColumnName("title").IsRequired().HasMaxLength(250);
        builder.Property(t => t.Description).HasColumnName("description").HasMaxLength(1000);
        builder.Property(t => t.Deadline).HasColumnName("deadline").IsRequired();
        builder.Property(t => t.Status).HasColumnName("status").HasConversion<string>().HasDefaultValue(CommunityTaskStatus.Active);

        // 3. Audit Properties
        builder.Property(t => t.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(t => t.UpdatedAt).HasColumnName("updated_at").IsRequired();

        // 4. Relationships
        builder.Property(t => t.CommunityId).HasColumnName("community_id");
        builder.Property(t => t.CategoryId).HasColumnName("category_id");

        builder.HasOne(t => t.Community).WithMany(c => c.Tasks).HasForeignKey(t => t.CommunityId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(t => t.Category).WithMany(cat => cat.Tasks).HasForeignKey(t => t.CategoryId).OnDelete(DeleteBehavior.SetNull);

        // 5. Indexes
        builder.HasIndex(t => t.CommunityId).HasDatabaseName("ix_community_tasks_community_id");
        builder.HasIndex(t => t.CategoryId).HasDatabaseName("ix_community_tasks_category_id");
    }
}
