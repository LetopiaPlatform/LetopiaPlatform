using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LetopiaPlatform.Core.Entities.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace LetopiaPlatform.Infrastructure.Data.Configurations;
public class PendingEmailChangeConfiguration : IEntityTypeConfiguration<PendingEmailChange>
{
    public void Configure(EntityTypeBuilder<PendingEmailChange> builder)
    {
        builder.ToTable("PendingEmailChanges");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.NewEmail).IsRequired().HasMaxLength(256);
        builder.Property(x => x.Token).IsRequired().HasMaxLength(512);
        builder.Property(x => x.ExpiresAt).IsRequired();
        builder.Property(x => x.IsUsed).HasDefaultValue(false);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UserId, x.IsUsed });
        builder.HasIndex(x => x.Token).IsUnique();
    }
}
