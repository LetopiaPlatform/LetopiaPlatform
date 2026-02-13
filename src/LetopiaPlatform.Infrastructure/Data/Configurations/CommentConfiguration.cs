using LetopiaPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LetopiaPlatform.Infrastructure.Data.Configurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        // Table
        builder.ToTable("comments");

        // Primary key
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");

        // Audit columns
        builder.Property(c => c.CreatedAt).HasColumnName("created_at");
        builder.Property(c => c.UpdatedAt).HasColumnName("updated_at");

        // Properties
        builder.Property(c => c.PostId)
            .HasColumnName("post_id")
            .IsRequired();

        builder.Property(c => c.AuthorId)
            .HasColumnName("author_id")
            .IsRequired();

        builder.Property(c => c.Content)
            .HasColumnName("content")
            .IsRequired();

        builder.Property(c => c.Upvotes)
            .HasColumnName("upvotes")
            .HasDefaultValue(0);

        // Indexes
        builder.HasIndex(c => c.PostId)
            .HasDatabaseName("ix_comments_post_id");

        builder.HasIndex(c => c.AuthorId)
            .HasDatabaseName("ix_comments_author_id");

        // Relationships
        builder.HasOne(c => c.Post)
            .WithMany(p => p.Comments)
            .HasForeignKey(c => c.PostId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Author)
            .WithMany()
            .HasForeignKey(c => c.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Matching query filter: exclude comments whose parent post is soft-deleted.
        // Required because Post has a global ISoftDeletable filter — without this,
        // EF Core warns about required navigation + filtered principal mismatch.
        builder.HasQueryFilter(c => !c.Post.IsDeleted);
    }
}