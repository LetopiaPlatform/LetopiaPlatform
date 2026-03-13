using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LetopiaPlatform.Infrastructure.Data.Configurations;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        // ── Table & primary key ───────────────────────────────────────────────

        builder.ToTable("posts");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");

        // ── Audit columns ─────────────────────────────────────────────────────

        builder.Property(p => p.CreatedAt).HasColumnName("created_at");
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at");

        // ── Scalar properties ─────────────────────────────────────────────────

        builder.Property(p => p.CommunityId)
               .HasColumnName("community_id")
               .IsRequired();

        builder.Property(p => p.ChannelId)
               .HasColumnName("channel_id")
               .IsRequired();

        builder.Property(p => p.AuthorId)
               .HasColumnName("author_id")
               .IsRequired();

        builder.Property(p => p.Title)
               .HasColumnName("title")
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(p => p.Content)
               .HasColumnName("content")
               .IsRequired();

        builder.Property(p => p.PostType)
               .HasColumnName("post_type")
               .HasMaxLength(20)
               .HasConversion<string>()
               .HasDefaultValue(PostType.Discussion);

        builder.Property(p => p.Upvotes)
               .HasColumnName("upvotes")
               .HasDefaultValue(0);

        builder.Property(p => p.CommentCount)
               .HasColumnName("comment_count")
               .HasDefaultValue(0);

        builder.Property(p => p.ViewsCount)
               .HasColumnName("views_count")
               .HasDefaultValue(0);

        builder.Property(p => p.IsPinned)
               .HasColumnName("is_pinned")
               .HasDefaultValue(false);

        builder.Property(p => p.IsDeleted)
               .HasColumnName("is_deleted")
               .HasDefaultValue(false);

        // ── ImageUrls — stored as a JSON array column ─────────────────────────
        // EF Core 8+ serializes List<string> to a single jsonb column natively.

        // Trade-off: cannot query by individual URL in SQL without JSON operators,
        // but for a read-all access pattern this is perfectly fine.

        builder.Property(p => p.ImageUrls)
               .HasColumnName("image_urls")
               .HasColumnType("text[]") 
               .HasDefaultValueSql("'{}'::text[]"); // empty array by default

        // ── Soft-delete global query filter ───────────────────────────────────

        builder.HasQueryFilter(p => !p.IsDeleted);

        // ── Indexes ───────────────────────────────────────────────────────────

        builder.HasIndex(p => new { p.CommunityId, p.CreatedAt })
               .IsDescending(false, true)
               .HasFilter("\"is_deleted\" = false")
               .HasDatabaseName("ix_posts_community_created");

        builder.HasIndex(p => p.AuthorId)
               .HasDatabaseName("ix_posts_author_id");

        builder.HasIndex(p => p.CommunityId)
               .HasDatabaseName("ix_posts_community_id");

        builder.HasIndex(p => p.ChannelId)
               .HasDatabaseName("ix_posts_channel_id");

        // ── Relationships ─────────────────────────────────────────────────────

        builder.HasOne(p => p.Community)
               .WithMany(c => c.Posts)
               .HasForeignKey(p => p.CommunityId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Channel)
               .WithMany(ch => ch.Posts)
               .HasForeignKey(p => p.ChannelId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Author)
               .WithMany()
               .HasForeignKey(p => p.AuthorId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Comments)
               .WithOne(c => c.Post)
               .HasForeignKey(c => c.PostId);
    }
}
