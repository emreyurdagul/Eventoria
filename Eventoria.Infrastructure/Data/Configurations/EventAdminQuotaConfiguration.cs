using Eventoria.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Eventoria.Infrastructure.Persistence.Configurations;

public sealed class EventAdminQuotaConfiguration : IEntityTypeConfiguration<EventAdminQuota>
{
    public void Configure(EntityTypeBuilder<EventAdminQuota> builder)
    {
        builder.ToTable("event_admin_quotas");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.AdminUserId).IsRequired();

        builder.Property(x => x.MaxEvents).IsRequired();
        builder.Property(x => x.MaxTotalParticipants).IsRequired();

        builder.Property(x => x.IsActive).IsRequired();

        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.UpdatedAtUtc);
        builder.Property(x => x.ConcurrencyStamp)
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(x => x.AdminUserId);
        builder.HasIndex(x => new { x.AdminUserId, x.IsActive });
    }
}
