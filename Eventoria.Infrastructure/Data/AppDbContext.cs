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

        // Audit + concurrency
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                // BaseEntity Id domain tarafından set ediliyor; null ise burada setleyebilirsin
                if (entry.Entity.Id == Guid.Empty)
                    entry.Entity.GetType().GetProperty("Id")?.SetValue(entry.Entity, Guid.NewGuid());

                entry.Entity.GetType().GetProperty("CreatedAtUtc")?.SetValue(entry.Entity, now);
                entry.Entity.GetType().GetProperty("UpdatedAtUtc")?.SetValue(entry.Entity, null);
                entry.Entity.GetType().GetProperty("ConcurrencyStamp")?.SetValue(entry.Entity, Guid.NewGuid().ToString("N"));
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.GetType().GetProperty("UpdatedAtUtc")?.SetValue(entry.Entity, now);
                entry.Entity.GetType().GetProperty("ConcurrencyStamp")?.SetValue(entry.Entity, Guid.NewGuid().ToString("N"));
            }
        }

        // Domain events (collect before save)
        var domainEvents = ChangeTracker.Entries<BaseEntity>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        // Clear after save
        foreach (var e in ChangeTracker.Entries<BaseEntity>())
            e.Entity.ClearDomainEvents();

        // Dispatch events (sync placeholder)
        // Burayı ileride MediatR / message bus / outbox ile profesyonelce büyüteceğiz.
        // Şimdilik DI ile bir dispatcher çağıracağız.
        if (domainEvents.Count > 0)
        {
            var dispatcher = this.GetService<IDomainEventDispatcher>();
            if (dispatcher != null)
                await dispatcher.DispatchAsync(domainEvents, cancellationToken);
        }

        return result;
    }
}
