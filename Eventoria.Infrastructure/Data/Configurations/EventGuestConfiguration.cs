using Eventoria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Eventoria.Infrastructure.Persistence.Configurations;

public sealed class EventGuestConfiguration : IEntityTypeConfiguration<EventGuest>
{
    public void Configure(EntityTypeBuilder<EventGuest> b)
    {
        b.ToTable("EventGuests");

        b.HasKey(x => x.Id);

        b.Property(x => x.DisplayName)
            .HasMaxLength(120)
            .IsRequired();

        b.HasIndex(x => new { x.EventId, x.Id });
    }
}
