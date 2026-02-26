using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LetopiaPlatform.Infrastructure.Data.Configurations;

public class ReactionConfiguration : IEntityTypeConfiguration<Reaction>
{
    public void Configure(EntityTypeBuilder<Reaction> builder)
    {
        // Table name
        builder.ToTable("reactions");

        // Primary key from BaseEntity.Id
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");

        // Columns
        builder.Property(r => r.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(r => r.TargetType)
            .HasColumnName("target_type")
            .HasMaxLength(50)
            .IsRequired(false); // nullable as per your entity

        builder.Property(r => r.TargetId)
            .HasColumnName("target_id")
            .IsRequired();

        builder.Property(r => r.ReactionType)
            .HasColumnName("reaction_type")
            .HasMaxLength(20)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes
        builder.HasIndex(r => new { r.UserId, r.TargetType, r.TargetId })
            .IsUnique()
            .HasDatabaseName("ux_reactions_user_target");

        builder.HasIndex(r => new { r.TargetType, r.TargetId })
            .HasDatabaseName("ix_reactions_target");

        // Relationships
        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
