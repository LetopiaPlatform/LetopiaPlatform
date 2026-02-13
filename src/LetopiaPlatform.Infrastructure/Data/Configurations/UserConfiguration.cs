using LetopiaPlatform.Core.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LetopiaPlatform.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Table is already mapped to "AspNetUsers" in ApplicationDbContext

        // Properties
        builder.Property(u => u.FullName)
            .HasColumnName("FullName");

        builder.Property(u => u.AvatarUrl)
            .HasColumnName("AvatarUrl");

        builder.Property(u => u.Bio)
            .HasColumnName("Bio");

        builder.Property(u => u.Role)
            .HasColumnName("Role")
            .IsRequired()
            .HasDefaultValue("Learner");

        builder.Property(u => u.EmailVerified)
            .HasColumnName("EmailVerified")
            .HasDefaultValue(false);

        builder.Property(u => u.TotalPoints)
            .HasColumnName("TotalPoints")
            .HasDefaultValue(0);

        builder.Property(u => u.CurrentStreak)
            .HasColumnName("CurrentStreak")
            .HasDefaultValue(0);

        builder.Property(u => u.LastLoginAt)
            .HasColumnName("LastLoginAt");

        builder.Property(u => u.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("UpdatedAt")
            .IsRequired();
    }
}
