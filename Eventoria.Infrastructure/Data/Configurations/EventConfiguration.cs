using Eventoria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Eventoria.Infrastructure.Data.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("events");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .HasMaxLength(12)
            .IsRequired();

        builder.HasIndex(x => x.Code).IsUnique();

        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.CreatedByUserId).IsRequired();

        builder.Property(x => x.CoverPhotoMediaFileId);


        // ? Owned: EventSpecs -> events tablosuna kolon
        builder.OwnsOne(x => x.Specs, specs =>
        {
            specs.Property(p => p.ParticipantLimit)
                .HasColumnName("participant_limit")
                .IsRequired();

            specs.Property(p => p.PhotosPerUserLimit)
                .HasColumnName("photos_per_user_limit")
                .IsRequired();

            specs.Property(p => p.VideosPerUserLimit)
                .HasColumnName("videos_per_user_limit")
                .IsRequired();
        });

        // BaseEntity fields
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.UpdatedAtUtc);
        builder.Property(x => x.ConcurrencyStamp)
            .HasMaxLength(64)
            .IsConcurrencyToken()
            .IsRequired();

        // IMPORTANT: Use navigation mapping + Field access (do NOT also map by field name)
        builder.Navigation(x => x.Memberships)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(x => x.Invites)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.Memberships)
            .WithOne()
            .HasForeignKey(nameof(EventMembership.EventId))
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Invites)
            .WithOne()
            .HasForeignKey(nameof(EventInvite.EventId))
            .OnDelete(DeleteBehavior.Cascade);
    }
}
