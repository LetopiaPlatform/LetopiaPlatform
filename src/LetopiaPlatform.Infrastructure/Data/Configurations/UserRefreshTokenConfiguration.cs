using LetopiaPlatform.Core.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LetopiaPlatform.Infrastructure.Data.Configurations;

public class UserRefreshTokenConfiguration : IEntityTypeConfiguration<UserRefreshToken>
{
    public void Configure(EntityTypeBuilder<UserRefreshToken> builder)
    {
        // 1. Table & Primary Key
        builder.ToTable("user_refresh_tokens");
        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Id)
            .HasColumnName("id");

        // 2. Properties Mapping (Snake Case)
        builder.Property(rt => rt.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(rt => rt.RefreshTokenHash)
            .HasColumnName("refresh_token_hash")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(rt => rt.JwtId)
            .HasColumnName("jwt_id")
            .IsRequired();

        // Concurrency Token: (Race Condition Protection)

        builder.Property(rt => rt.IsUsed)
            .HasColumnName("is_used")
            .HasDefaultValue(false)
            .IsConcurrencyToken();

        builder.Property(rt => rt.IsRevoked)
            .HasColumnName("is_revoked")
            .HasDefaultValue(false);

        builder.Property(rt => rt.AddedTime)
            .HasColumnName("added_time")
            .IsRequired();

        builder.Property(rt => rt.ExpiryDate)
            .HasColumnName("expiry_date")
            .IsRequired();

        // 3. Relationships

        builder.HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // 4. Indexes for high-performance lookups

        builder.HasIndex(rt => rt.RefreshTokenHash)
            .IsUnique()
            .HasDatabaseName("ix_user_refresh_tokens_hash");

        builder.HasIndex(rt => rt.UserId)
            .HasDatabaseName("ix_user_refresh_tokens_user_id");

        builder.HasIndex(rt => rt.JwtId)
            .HasDatabaseName("ix_user_refresh_tokens_jwt_id");

        // Composite Index: 
        builder.HasIndex(rt => new { rt.RefreshTokenHash, rt.UserId })
            .HasDatabaseName("ix_user_refresh_tokens_hash_user");
    }
}
