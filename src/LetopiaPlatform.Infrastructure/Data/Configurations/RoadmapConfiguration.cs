using LetopiaPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LetopiaPlatform.Infrastructure.Data.Configurations;

public class RoadmapConfiguration : IEntityTypeConfiguration<Roadmap>
{
    public void Configure(EntityTypeBuilder<Roadmap> builder)
    {
        // Table
        builder.ToTable("roadmaps");

        // Primary key
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");

        // Audit columns
        builder.Property(r => r.CreatedAt).HasColumnName("created_at");
        builder.Property(r => r.UpdatedAt).HasColumnName("updated_at");

        // Properties
        builder.Property(r => r.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(r => r.ConversationId)
            .HasColumnName("conversation_id")
            .IsRequired();

        builder.Property(r => r.Title)
            .HasColumnName("title")
            .IsRequired();

        builder.Property(r => r.Topic)
            .HasColumnName("topic")
            .IsRequired();

        builder.Property(r => r.Description)
            .HasColumnName("description")
            .IsRequired();

        builder.Property(r => r.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(r => r.EstimatedDurationWeeks)
            .HasColumnName("estimated_duration_weeks");

        // Indexes
        builder.HasIndex(r => r.UserId)
            .HasDatabaseName("ix_roadmaps_user_id");

        builder.HasIndex(r => r.ConversationId)
            .HasDatabaseName("ix_roadmaps_conversation_id");

        // Relationships
        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Conversation)
            .WithMany()
            .HasForeignKey(r => r.ConversationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.Phases)
            .WithOne(p => p.Roadmap)
            .HasForeignKey(p => p.RoadmapId);
    }
}
