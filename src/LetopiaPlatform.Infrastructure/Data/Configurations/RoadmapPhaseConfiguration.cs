using System.Text.Json;
using System.Text.Json.Serialization;
using LetopiaPlatform.Core.DTOs.Agent;
using LetopiaPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LetopiaPlatform.Infrastructure.Data.Configurations;

public class RoadmapPhaseConfiguration : IEntityTypeConfiguration<RoadmapPhase>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    public void Configure(EntityTypeBuilder<RoadmapPhase> builder)
    {
        // Table
        builder.ToTable("roadmap_phases");

        // Primary key
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");

        // Audit columns
        builder.Property(p => p.CreatedAt).HasColumnName("created_at");
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at");

        // Properties
        builder.Property(p => p.RoadmapId)
            .HasColumnName("roadmap_id")
            .IsRequired();

        builder.Property(p => p.Title)
            .HasColumnName("title")
            .IsRequired();

        builder.Property(p => p.Description)
            .HasColumnName("description")
            .IsRequired();

        builder.Property(p => p.Order)
            .HasColumnName("order");

        builder.Property(p => p.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(p => p.DurationEstimateWeeks)
            .HasColumnName("duration_estimate_weeks");

        // JSONB columns with System.Text.Json conversion
        builder.Property(p => p.Resources)
            .HasColumnName("resources")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<List<PhaseResource>>(v, JsonOptions) ?? new List<PhaseResource>());

        builder.Property(p => p.Projects)
            .HasColumnName("projects")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<List<PhaseProject>>(v, JsonOptions) ?? new List<PhaseProject>());

        builder.Property(p => p.Insights)
            .HasColumnName("insights")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<List<string>>(v, JsonOptions) ?? new List<string>());

        // Index
        builder.HasIndex(p => p.RoadmapId)
            .HasDatabaseName("ix_roadmap_phases_roadmap_id");

        // Relationships
        builder.HasOne(p => p.Roadmap)
            .WithMany(r => r.Phases)
            .HasForeignKey(p => p.RoadmapId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
