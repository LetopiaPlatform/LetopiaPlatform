using LetopiaPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LetopiaPlatform.Infrastructure.Data.Configurations;
public class ProjectMemberConfiguration : IEntityTypeConfiguration<ProjectMember>
{
    public void Configure(EntityTypeBuilder<ProjectMember> builder)
    {

        builder.HasKey(pm => new { pm.ProjectId, pm.MemberId });


        builder.Property(pm => pm.Role)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();


        builder.HasOne(pm => pm.Project)
            .WithMany(p => p.Members)
            .HasForeignKey(pm => pm.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);


        builder.HasOne(pm => pm.Member)
            .WithMany(u => u.ProjectMemberships)
            .HasForeignKey(pm => pm.MemberId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
