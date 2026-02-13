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

    public Task<Event?> GetByIdWithIncludesAsync(Guid eventId, CancellationToken ct)
        => _db.Events
            .Include(e => e.Invites)
            .Include(e => e.Memberships)
            .FirstOrDefaultAsync(e => e.Id == eventId, ct);

    public Task<Event?> GetByIdWithIncludesAsNoTrackingAsync(Guid eventId, CancellationToken ct)
        => _db.Events
            .AsNoTracking()
            .Include(e => e.Invites)
            .Include(e => e.Memberships)
            .FirstOrDefaultAsync(e => e.Id == eventId, ct);

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct)
        => _db.Events.AnyAsync(e => e.Code == code, ct);

    public Task<Event?> GetByCodeAsync(string code, CancellationToken ct)
        => _db.Events
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

    public Task<int> SumParticipantLimitsCreatedByAsync(Guid? adminUserId, CancellationToken ct)
        => _db.Events
            .Where(e => e.CreatedByUserId == adminUserId)
            .SumAsync(e => e.Specs.ParticipantLimit, ct);

    public async Task<int> SumParticipantLimitsCreatedByExcludingEventAsync(
        Guid creatorUserId,
        Guid excludeEventId,
        CancellationToken ct)
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
            .AsNoTracking()
            .Where(m => m.UserId == userId)
            .Join(
                _db.Events.AsNoTracking(),
                m => m.EventId,
                e => e.Id,
                (m, e) => new { m, e }
            )
            .Select(x => new MyEventItem(
                EventId: x.e.Id,
                Code: x.e.Code,
                Title: x.e.Title,
                Date: x.e.Date,
                Status: x.e.Status,
                MyRole: x.m.Role,
                ParticipantLimit: x.e.Specs.ParticipantLimit,
                MemberCount: _db.EventMemberships.Count(mm => mm.EventId == x.e.Id),
                CoverPhotoMediaFileId: x.e.CoverPhotoMediaFileId   // ✅ BURASI Event’ten
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

        var list = await _db.EventMemberships
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
                MemberCount: _db.EventMemberships.Count(x => x.EventId == m.EventId),
                CoverPhotoMediaFileId: m.Event.CoverPhotoMediaFileId
            ))
            .ToListAsync(ct);

        return list;
    }

    public async Task<IReadOnlyList<MyEventItem>> GetAllEventsPagedAsync(
        int page,
        int pageSize,
        CancellationToken ct)
    {
        var skip = (page - 1) * pageSize;

        var list = await _db.Events
            .AsNoTracking()
            .OrderByDescending(e => e.CreatedAtUtc)
            .Skip(skip)
            .Take(pageSize)
            .Select(e => new MyEventItem(
                EventId: e.Id,
                Code: e.Code,
                Title: e.Title,
                Date: e.Date,
                Status: e.Status,
                MyRole: EventRole.Admin,
                ParticipantLimit: e.Specs.ParticipantLimit,
                MemberCount: _db.EventMemberships.Count(x => x.EventId == e.Id),
                CoverPhotoMediaFileId: e.CoverPhotoMediaFileId
            ))
            .ToListAsync(ct);

        return list;
    }


    public Task<int> CountMyEventsAsync(Guid userId, CancellationToken ct)
        => _db.EventMemberships.CountAsync(m => m.UserId == userId, ct);

    public Task<int> CountAllEventsAsync(CancellationToken ct)
        => _db.Events.CountAsync(ct);

    public async Task<EventDetailsDto?> GetEventDetailsAsync(Guid eventId, Guid userId, CancellationToken ct)
    {
        var roleRow = await _db.EventMemberships
            .AsNoTracking()
            .Where(m => m.EventId == eventId && m.UserId == userId)
            .Select(m => new { m.Role })
            .FirstOrDefaultAsync(ct);

        if (roleRow is null)
            return null;

        var role = roleRow.Role;

        // Not: ctor'da "cover" isimli parametre yok => named arg KULLANMIYORUZ
        return await _db.Events
            .AsNoTracking()
            .Where(e => e.Id == eventId)
            .Select(e => new EventDetailsDto(
                e.Id,
                e.Code,
                e.Title,
                e.Description,
                e.Date,
                e.Status,
                e.CreatedByUserId,
                /* EventCoverDto? */ null, // ✅ cover parametresi, ismi ne olursa olsun sırayla
                role,
                e.Specs.ParticipantLimit,
                e.Specs.PhotosPerUserLimit,
                e.Specs.VideosPerUserLimit,
                _db.EventMemberships.Count(m => m.EventId == e.Id),
                role == EventRole.Admin
                    ? _db.Set<EventInvite>().Any(i => i.EventId == e.Id && i.IsActive)
                    : false
            ))
            .FirstOrDefaultAsync(ct);
    }

    public Task<EventDetailsDto?> GetEventDetailsByIdAsync(Guid eventId, CancellationToken ct)
    {
        // Not: ctor'da "cover" isimli parametre yok => named arg KULLANMIYORUZ
        return _db.Events
            .AsNoTracking()
            .Where(e => e.Id == eventId)
            .Select(e => new EventDetailsDto(
                e.Id,
                e.Code,
                e.Title,
                e.Description,
                e.Date,
                e.Status,
                e.CreatedByUserId,
                /* EventCoverDto? */ null, // ✅ cover parametresi
                EventRole.Admin,
                e.Specs.ParticipantLimit,
                e.Specs.PhotosPerUserLimit,
                e.Specs.VideosPerUserLimit,
                _db.EventMemberships.Count(m => m.EventId == e.Id),
                _db.Set<EventInvite>().Any(i => i.EventId == e.Id && i.IsActive)
            ))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PagedResult<EventMemberDto>> GetEventMembersAsync(
        Guid eventId,
        int page,
        int pageSize,
        CancellationToken ct)
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
                _db.Users.AsNoTracking(),
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

    public async Task DeactivateActiveInvitesAsync(Guid eventId, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        await _db.EventInvites
            .Where(i => i.EventId == eventId && i.IsActive)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(i => i.IsActive, false)
                .SetProperty(i => i.RotatedAtUtc, now)
                .SetProperty(i => i.UpdatedAtUtc, now)
                .SetProperty(i => i.ConcurrencyStamp, Guid.NewGuid().ToString("N")),
                ct);
    }
}
