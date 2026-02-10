using Eventoria.Application.Abstractions.Persistence;
using Eventoria.Application.Admin.Events.Models;
using Eventoria.Application.Common.Models;
using Eventoria.Application.Events.Queries.Models;
using Eventoria.Domain.Entities;
using Eventoria.Domain.Enums;
using Eventoria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Eventoria.Infrastructure.Persistence;

public class EventRepository : GenericRepository<Event>, IEventRepository
{
    private readonly AppDbContext _db;

    public EventRepository(AppDbContext db) : base(db)
    {
        _db = db;
    }

    public async Task<Event?> GetByIdWithIncludesAsync(Guid eventId, CancellationToken ct)
    => await _db.Events
        .Include(e => e.Invites)
        .Include(e => e.Memberships)
        .FirstOrDefaultAsync(e => e.Id == eventId, ct);

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct)
        => _db.Events.AnyAsync(e => e.Code == code, ct);

    public async Task<Event?> GetByCodeAsync(string code, CancellationToken ct)
        => await _db.Events
            .Include(e => e.Invites)
            .Include(e => e.Memberships)
            .FirstOrDefaultAsync(e => e.Code == code, ct);

    public Task<bool> IsEventAdminAsync(Guid eventId, Guid userId, CancellationToken ct)
        => _db.EventMemberships.AnyAsync(m =>
            m.EventId == eventId &&
            m.UserId == userId &&
            m.Role == EventRole.Admin, ct);

    public Task<int> CountCreatedByAsync(Guid? adminUserId, CancellationToken ct)
    => _db.Events.CountAsync(e => e.CreatedByUserId == adminUserId, ct);

    public async Task<int> SumParticipantLimitsCreatedByAsync(Guid? adminUserId, CancellationToken ct)
    {
        return await _db.Events
            .Where(e => e.CreatedByUserId == adminUserId)
            .SumAsync(e => e.Specs.ParticipantLimit, ct);
    }

    public async Task<int> SumParticipantLimitsCreatedByExcludingEventAsync(Guid creatorUserId, Guid excludeEventId, CancellationToken ct)
    {
        var sum = await _db.Events
            .Where(e => e.CreatedByUserId == creatorUserId && e.Id != excludeEventId)
            .SumAsync(e => (int?)e.Specs.ParticipantLimit, ct);

        return sum ?? 0;
    }

    public Task<bool> IsMemberAsync(Guid eventId, Guid userId, CancellationToken ct)
    => _db.EventMemberships.AnyAsync(m => m.EventId == eventId && m.UserId == userId, ct);

    public async Task<IReadOnlyList<MyEventItem>> GetMyEventsAsync(Guid userId, CancellationToken ct)
    {
        var list = await _db.EventMemberships
            .Where(m => m.UserId == userId)
            .Join(
                _db.Events,
                m => m.EventId,
                e => e.Id,
                (m, e) => new { m, e }
            )
            .Select(x => new MyEventItem(
                x.e.Id,
                x.e.Code,
                x.e.Title,
                x.e.Date,
                x.e.Status,
                x.m.Role,
                x.e.Specs.ParticipantLimit,
                _db.EventMemberships.Count(mm => mm.EventId == x.e.Id)
            ))
            .OrderByDescending(x => x.Date)
            .ToListAsync(ct);

        return list;
    }

    public async Task<IReadOnlyList<MyEventItem>> GetMyEventsPagedAsync(
       Guid userId,
       int page,
       int pageSize,
       CancellationToken ct)
    {
        var skip = (page - 1) * pageSize;

        return await _db.EventMemberships
            .AsNoTracking()
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.Event.CreatedAtUtc)
            .Skip(skip)
            .Take(pageSize)
            .Select(m => new MyEventItem(
                EventId: m.Event.Id,
                Code: m.Event.Code,
                Title: m.Event.Title,
                Date: m.Event.Date,
                Status: m.Event.Status,
                MyRole: m.Role,
                ParticipantLimit: m.Event.Specs.ParticipantLimit,
                MemberCount: _db.EventMemberships.Count(x => x.EventId == m.EventId)
            ))
            .ToListAsync(ct);
    }

    public async Task<EventDetailsDto?> GetEventDetailsAsync(Guid eventId, Guid userId, CancellationToken ct)
    {
        var row = await _db.EventMemberships
            .Where(m => m.EventId == eventId && m.UserId == userId)
            .Select(m => new { m.Role })
            .FirstOrDefaultAsync(ct);

        if (row == null)
            return null;

        var role = row.Role;

        var dto = await _db.Events
            .Where(e => e.Id == eventId)
            .Select(e => new EventDetailsDto(
                e.Id,
                e.Code,
                e.Title,
                e.Description,
                e.Date,
                e.Status,
                e.CreatedByUserId,
                role,
                e.Specs.ParticipantLimit,
                e.Specs.PhotosPerUserLimit,
                e.Specs.VideosPerUserLimit,
                _db.EventMemberships.Count(m => m.EventId == e.Id),
                role == Domain.Enums.EventRole.Admin
                    ? _db.Set<Domain.Entities.EventInvite>().Any(i => i.EventId == e.Id && i.IsActive)
                    : false
            ))
            .FirstOrDefaultAsync(ct);

        return dto;
    }

    public async Task<PagedResult<EventMemberDto>> GetEventMembersAsync(Guid eventId, int page, int pageSize, CancellationToken ct)
    {
        var query = _db.EventMemberships
            .AsNoTracking()
            .Where(m => m.EventId == eventId);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(m => m.JoinedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Join(
                _db.Users,
                m => m.UserId,
                u => u.Id,
                (m, u) => new EventMemberDto(
                    u.Id,
                    u.Email ?? string.Empty,
                    u.DisplayName,
                    m.Role,
                    m.JoinedAtUtc
                )
            )
            .ToListAsync(ct);

        return new PagedResult<EventMemberDto>(items, page, pageSize, total);
    }
}
