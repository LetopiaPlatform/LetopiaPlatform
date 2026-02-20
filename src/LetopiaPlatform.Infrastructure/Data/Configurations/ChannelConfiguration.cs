using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LetopiaPlatform.Infrastructure.Data.Configurations;

public class ChannelConfiguration : IEntityTypeConfiguration<Channel>
{
    public void Configure(EntityTypeBuilder<Channel> builder)
    {
        // Table
        builder.ToTable("channels");

        // Primary key
        builder.HasKey(ch => ch.Id);
        builder.Property(ch => ch.Id).HasColumnName("id");

        // Audit columns
        builder.Property(ch => ch.CreatedAt).HasColumnName("created_at");
        builder.Property(ch => ch.UpdatedAt).HasColumnName("updated_at");

        // Properties
        builder.Property(ch => ch.CommunityId)
            .HasColumnName("community_id")
            .IsRequired();

        builder.Property(ch => ch.ParentId)
            .HasColumnName("parent_id");

        builder.Property(ch => ch.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(ch => ch.Slug)
            .HasColumnName("slug")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(ch => ch.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(ch => ch.ChannelType)
            .HasColumnName("channel_type")
            .HasMaxLength(20)
            .HasConversion<string>()
            .HasDefaultValue(ChannelType.Discussion)
            .HasSentinel(ChannelType.Discussion);

        builder.Property(ch => ch.DisplayOrder)
            .HasColumnName("display_order")
            .HasDefaultValue(0);

        builder.Property(ch => ch.PostCount)
            .HasColumnName("post_count")
            .HasDefaultValue(0);

        builder.Property(ch => ch.IsDefault)
            .HasColumnName("is_default")
            .HasDefaultValue(false);

        builder.Property(ch => ch.IsArchived)
            .HasColumnName("is_archived")
            .HasDefaultValue(false);

        builder.Property(ch => ch.AllowMemberPosts)
            .HasColumnName("allow_member_posts")
            .HasDefaultValue(true);

        builder.Property(ch => ch.AllowComments)
            .HasColumnName("allow_comments")
            .HasDefaultValue(true);

        // Indexes
        builder.HasIndex(ch => ch.CommunityId)
            .HasDatabaseName("ix_channels_community_id");

        builder.HasIndex(ch => new { ch.CommunityId, ch.ParentId, ch.Slug })
            .IsUnique()
            .HasDatabaseName("ix_channels_community_parent_slug");

        // Relationships
        builder.HasOne(ch => ch.Community)
            .WithMany(c => c.Channels)
            .HasForeignKey(ch => ch.CommunityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ch => ch.Parent)
            .WithMany(ch => ch.SubChannels)
            .HasForeignKey(ch => ch.ParentId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(ch => ch.Posts)
            .WithOne(p => p.Channel)
            .HasForeignKey(p => p.ChannelId);
    }
}