using Eventoria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Eventoria.Infrastructure.Persistence.Configurations;

public sealed class PostMediaConfiguration : IEntityTypeConfiguration<PostMedia>
{
    public void Configure(EntityTypeBuilder<PostMedia> builder)
    {
        builder.ToTable("post_media");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.PostId).IsRequired();
        builder.Property(x => x.MediaFileId).IsRequired();
        builder.Property(x => x.Order).IsRequired();

        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.UpdatedAtUtc);
        builder.Property(x => x.ConcurrencyStamp).HasMaxLength(32).IsRequired();

        builder.HasIndex(x => new { x.PostId, x.Order }).IsUnique();
        builder.HasIndex(x => new { x.PostId, x.MediaFileId }).IsUnique();
    }
}

