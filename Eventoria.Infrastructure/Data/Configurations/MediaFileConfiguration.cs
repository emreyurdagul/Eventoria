using Eventoria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Eventoria.Infrastructure.Persistence.Configurations;

public sealed class MediaFileConfiguration : IEntityTypeConfiguration<MediaFile>
{
    public void Configure(EntityTypeBuilder<MediaFile> builder)
    {
        builder.ToTable("media_files");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.OwnerUserId).IsRequired();
        builder.Property(x => x.EventId);

        builder.Property(x => x.Visibility).IsRequired();
        builder.Property(x => x.Status).IsRequired();

        builder.Property(x => x.ProviderKey).HasMaxLength(32).IsRequired();
        builder.Property(x => x.BucketOrContainer).HasMaxLength(128).IsRequired();
        builder.Property(x => x.ObjectKey).HasMaxLength(1024).IsRequired();

        builder.Property(x => x.OriginalFileName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(128).IsRequired();
        builder.Property(x => x.SizeBytes).IsRequired();

        builder.Property(x => x.ETag).HasMaxLength(128);
        builder.Property(x => x.Sha256).HasMaxLength(64);

        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.UpdatedAtUtc);
        builder.Property(x => x.ConcurrencyStamp).HasMaxLength(32).IsRequired();

        builder.HasIndex(x => new { x.EventId, x.CreatedAtUtc });
        builder.HasIndex(x => new { x.OwnerUserId, x.CreatedAtUtc });

        builder.HasIndex(x => new { x.ProviderKey, x.BucketOrContainer, x.ObjectKey }).IsUnique();
    }
}
