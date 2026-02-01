using Eventoria.Domain.Billing;
using Eventoria.Domain.Common;
using Eventoria.Domain.Entities;
using Eventoria.Domain.Enums;
using Eventoria.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Eventoria.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventMembership> EventMemberships => Set<EventMembership>();
    public DbSet<EventInvite> EventInvites => Set<EventInvite>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<EventAdminQuota> EventAdminQuotas => Set<EventAdminQuota>();
    public DbSet<MediaFile> MediaFiles => Set<MediaFile>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<PostMedia> PostMedias => Set<PostMedia>();
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply configurations (backing field mapping)
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Enum conversions (centralized)
        builder.Entity<Event>()
            .Property(x => x.Status)
            .HasConversion<int>();

        builder.Entity<EventMembership>()
            .Property(x => x.Role)
            .HasConversion<int>();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.Id == Guid.Empty)
                    entry.Entity.GetType().GetProperty(nameof(BaseEntity.Id))?
                        .SetValue(entry.Entity, Guid.NewGuid()); // Id protected set → bu satır da patlayabilir
                                                                 // Bu yüzden Id'yi de EF Property ile set edelim:
                if (entry.Property(nameof(BaseEntity.Id)).CurrentValue is Guid id && id == Guid.Empty)
                    entry.Property(nameof(BaseEntity.Id)).CurrentValue = Guid.NewGuid();

                entry.Property(nameof(BaseEntity.CreatedAtUtc)).CurrentValue = now;
                entry.Property(nameof(BaseEntity.UpdatedAtUtc)).CurrentValue = null;
                entry.Property(nameof(BaseEntity.ConcurrencyStamp)).CurrentValue = Guid.NewGuid().ToString("N");
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(nameof(BaseEntity.UpdatedAtUtc)).CurrentValue = now;
                entry.Property(nameof(BaseEntity.ConcurrencyStamp)).CurrentValue = Guid.NewGuid().ToString("N");
            }
        }

        // Domain events (collect before save)
        var domainEvents = ChangeTracker.Entries<BaseEntity>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        foreach (var e in ChangeTracker.Entries<BaseEntity>())
            e.Entity.ClearDomainEvents();

        if (domainEvents.Count > 0)
        {
            var dispatcher = this.GetService<IDomainEventDispatcher>();
            if (dispatcher != null)
                await dispatcher.DispatchAsync(domainEvents, cancellationToken);
        }

        return result;
    }
}
