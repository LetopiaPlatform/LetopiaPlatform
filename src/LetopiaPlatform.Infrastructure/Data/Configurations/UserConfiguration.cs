using LetopiaPlatform.Core.Entities.Identity;
using LetopiaPlatform.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LetopiaPlatform.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // ── Basic Profile ────────────────────────────────────────────────
        builder.Property(u => u.FullName)
            .HasColumnName("FullName")
            .HasMaxLength(200);

        builder.Property(u => u.AvatarUrl)
            .HasColumnName("AvatarUrl")
            .HasMaxLength(500);

        builder.Property(u => u.Bio)
            .HasColumnName("Bio")
            .HasMaxLength(1000);

        builder.Property(u => u.Role)
            .HasColumnName("Role")
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Learner");

        builder.Property(u => u.EmailVerified)
            .HasColumnName("EmailVerified")
            .HasDefaultValue(false);

        // ── Privacy & Visibility ─────────────────────────────────────────
        builder.OwnsOne(u => u.PrivacySettings, ps =>
        {
            ps.ToJson();
        });

        builder.Property(u => u.Location)
            .HasColumnName("Location")
            .HasMaxLength(100);

        // ── Notification Preferences (owned entity / JSON column) ────────
        builder.OwnsOne(u => u.NotificationPreferences, np =>
        {
            np.ToJson();
        });

        // ── Social Links (collection of owned entities → JSON column) ────
        builder.OwnsMany(u => u.SocialLinks, sl =>
        {
            sl.ToJson();
        });

        // ── Skills ───────────────────────────────────────────────────────
        builder.Property(u => u.Skills)
            .HasColumnName("skills")
            .HasColumnType("text[]")
            .HasDefaultValueSql("'{}'::text[]");

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Users_Skills_Unique",
            "NOT array_has_duplicates(skills)"
        ));

        // ── Interests ────────────────────────────────────────────────────
        builder.Property(u => u.Interests)
            .HasColumnName("interests")
            .HasColumnType("text[]")
            .HasDefaultValueSql("'{}'::text[]");

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Users_Interests_Unique",
            "NOT array_has_duplicates(interests)"
        ));

        // ── Gamification ─────────────────────────────────────────────────
        builder.Property(u => u.TotalPoints)
            .HasColumnName("TotalPoints")
            .HasDefaultValue(0);

        builder.Property(u => u.CurrentStreak)
            .HasColumnName("CurrentStreak")
            .HasDefaultValue(0);

        // ── Timestamps ───────────────────────────────────────────────────
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
