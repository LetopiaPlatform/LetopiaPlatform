using LetopiaPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LetopiaPlatform.Infrastructure.Data.Configurations;

public class ConversationMessageConfiguration : IEntityTypeConfiguration<ConversationMessage>
{
    public void Configure(EntityTypeBuilder<ConversationMessage> builder)
    {
        // Table
        builder.ToTable("conversation_messages");

        // Primary key
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id");

        // Properties
        builder.Property(m => m.ConversationId)
            .HasColumnName("conversation_id")
            .IsRequired();

        builder.Property(m => m.Role)
            .HasColumnName("role")
            .HasMaxLength(20)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(m => m.Content)
            .HasColumnName("content")
            .IsRequired();

        builder.Property(m => m.TokenCount)
            .HasColumnName("token_count");

        builder.Property(m => m.CreatedAt)
            .HasColumnName("created_at");

        // Index
        builder.HasIndex(m => m.ConversationId)
            .HasDatabaseName("ix_conversation_messages_conversation_id");

        // Relationships
        builder.HasOne(m => m.Conversation)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
