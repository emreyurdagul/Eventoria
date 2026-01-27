using Eventoria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Eventoria.Infrastructure.Data.Configurations;

public class EventInviteConfiguration : IEntityTypeConfiguration<EventInvite>
{
    public void Configure(EntityTypeBuilder<EventInvite> b)
    {
        b.ToTable("event_invites");

        b.HasKey(x => x.Id);

        b.Property(x => x.EventId).IsRequired();

        b.Property(x => x.InviteKeyHash)
            .HasMaxLength(128)
            .IsRequired();

        b.Property(x => x.IsActive).IsRequired();
        b.Property(x => x.RotatedAtUtc);

        // BaseEntity fields
        b.Property(x => x.CreatedAtUtc).IsRequired();
        b.Property(x => x.UpdatedAtUtc);
        b.Property(x => x.ConcurrencyStamp)
            .HasMaxLength(64)
            .IsConcurrencyToken()
            .IsRequired();

        // "Tek aktif invite" kuralı için Postgres partial index:
        b.HasIndex(x => new { x.EventId, x.IsActive })
            .HasFilter("\"IsActive\" = true");
    }
}
