using LetopiaPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LetopiaPlatform.Infrastructure.Data.Configurations;
public class UserTaskProgressConfiguration : IEntityTypeConfiguration<UserTaskProgress>
{
    public void Configure(EntityTypeBuilder<UserTaskProgress> builder)
    {
        // 1. Table & Composite Key
        builder.ToTable("user_task_progress");
        builder.HasKey(utp => new { utp.UserId, utp.TaskId });
        builder.Ignore(utp => utp.Id);
        // 2. Basic Properties
        builder.Property(utp => utp.IsCompleted).HasColumnName("is_completed").HasDefaultValue(false);
        builder.Property(utp => utp.CompletedAt).HasColumnName("completed_at");

        // 3. Audit Properties
        builder.Property(utp => utp.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(utp => utp.UpdatedAt).HasColumnName("updated_at").IsRequired();

        // 4. Relationships
        builder.Property(utp => utp.UserId).HasColumnName("user_id");
        builder.Property(utp => utp.TaskId).HasColumnName("task_id");

        builder.HasOne(utp => utp.User).WithMany().HasForeignKey(utp => utp.UserId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(utp => utp.Task).WithMany(t => t.UserProgresses).HasForeignKey(utp => utp.TaskId).OnDelete(DeleteBehavior.Cascade);
    }
}
