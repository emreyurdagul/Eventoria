using Eventoria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Eventoria.Infrastructure.Data.Configurations;

public class EventMembershipConfiguration : IEntityTypeConfiguration<EventMembership>
{
    public void Configure(EntityTypeBuilder<EventMembership> b)
    {
        b.ToTable("event_memberships");

        b.HasKey(x => x.Id);

        b.Property(x => x.EventId).IsRequired();
        b.Property(x => x.UserId).IsRequired();
        b.Property(x => x.Role).IsRequired();
        b.Property(x => x.JoinedAtUtc).IsRequired();

        // Foreign key to Event
        b.HasOne(x => x.Event)
            .WithMany(e => e.Memberships)
            .HasForeignKey(x => x.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique constraint: bir kullanici bir event'e sadece bir kez katilabilir
        b.HasIndex(x => new { x.EventId, x.UserId }).IsUnique();

        // BaseEntity fields
        b.Property(x => x.CreatedAtUtc).IsRequired();
        b.Property(x => x.UpdatedAtUtc);
        b.Property(x => x.ConcurrencyStamp)
            .HasMaxLength(64)
            .IsConcurrencyToken()
            .IsRequired();
    }
}
