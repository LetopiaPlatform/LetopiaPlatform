using LetopiaPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LetopiaPlatform.Infrastructure.Data.Configurations;

public class UserCommunityConfiguration : IEntityTypeConfiguration<UserCommunity>
{
    public void Configure(EntityTypeBuilder<UserCommunity> builder)
    {
        // Table
        builder.ToTable("user_communities");

        // Primary key
        builder.HasKey(uc => uc.Id);
        builder.Property(uc => uc.Id).HasColumnName("id");

        // Properties
        builder.Property(uc => uc.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(uc => uc.CommunityId)
            .HasColumnName("community_id")
            .IsRequired();

        builder.Property(uc => uc.JoinedAt)
            .HasColumnName("joined_at");

        // Indexes
        builder.HasIndex(uc => new { uc.UserId, uc.CommunityId })
            .IsUnique()
            .HasDatabaseName("ix_user_communities_user_community");

        // Relationships
        builder.HasOne(uc => uc.User)
            .WithMany()
            .HasForeignKey(uc => uc.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(uc => uc.Community)
            .WithMany(c => c.Members)
            .HasForeignKey(uc => uc.CommunityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}