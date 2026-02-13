using Eventoria.Application.Abstractions.Persistence;
using Eventoria.Application.Abstractions.Storage;
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
    private readonly IStorageProviderResolver _storageResolver;

    public EventRepository(AppDbContext db, IStorageProviderResolver storageResolver) : base(db)
    {
        _db = db;
        _storageResolver = storageResolver;
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

    private async Task<EventCoverDto?> GetCoverPhotoDtoAsync(Guid? mediaFileId, CancellationToken ct)
    {
        if (!mediaFileId.HasValue)
            return null;

        var mediaFile = await _db.Set<MediaFile>()
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == mediaFileId.Value, ct);

        if (mediaFile == null)
            return null;

        try
        {
            var storage = _storageResolver.Resolve(mediaFile.ProviderKey);
            var url = await storage.GetDownloadUrlAsync(
                mediaFile.BucketOrContainer,
                mediaFile.ObjectKey,
                validFor: TimeSpan.FromMinutes(15),
                ct);

            return new EventCoverDto(
                MediaFileId: mediaFile.Id,
                Url: url,
                ContentType: mediaFile.ContentType
            );
        }
        catch
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<MyEventItem>> GetMyEventsAsync(Guid userId, CancellationToken ct)
    {
        var eventIds = await _db.EventMemberships
            .AsNoTracking()
            .Where(m => m.UserId == userId)
            .Select(m => m.EventId)
            .ToListAsync(ct);

        var events = await _db.Events
            .AsNoTracking()
            .Where(e => eventIds.Contains(e.Id))
            .ToListAsync(ct);

        var memberships = await _db.EventMemberships
            .AsNoTracking()
            .Where(m => m.UserId == userId && eventIds.Contains(m.EventId))
            .ToListAsync(ct);

        var items = new List<MyEventItem>();
        foreach (var evt in events)
        {
            var membership = memberships.FirstOrDefault(m => m.EventId == evt.Id);
            var coverDto = await GetCoverPhotoDtoAsync(evt.CoverPhotoMediaFileId, ct);

            items.Add(new MyEventItem(
                EventId: evt.Id,
                Code: evt.Code,
                Title: evt.Title,
                Date: evt.Date,
                Status: evt.Status,
                MyRole: membership?.Role ?? EventRole.Participant,
                ParticipantLimit: evt.Specs.ParticipantLimit,
                MemberCount: await _db.EventMemberships.CountAsync(mm => mm.EventId == evt.Id, ct),
                CoverPhoto: coverDto
            ));
        }

        return items.OrderByDescending(x => x.Date).ToList();
    }

    public async Task<IReadOnlyList<MyEventItem>> GetMyEventsPagedAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken ct)
    {
        var skip = (page - 1) * pageSize;

        var eventIds = await _db.EventMemberships
            .AsNoTracking()
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.Event.CreatedAtUtc)
            .Skip(skip)
            .Take(pageSize)
            .Select(m => m.EventId)
            .ToListAsync(ct);

        var events = await _db.Events
            .AsNoTracking()
            .Where(e => eventIds.Contains(e.Id))
            .ToListAsync(ct);

        var memberships = await _db.EventMemberships
            .AsNoTracking()
            .Where(m => m.UserId == userId && eventIds.Contains(m.EventId))
            .ToListAsync(ct);

        var items = new List<MyEventItem>();
        foreach (var evt in events)
        {
            var membership = memberships.FirstOrDefault(m => m.EventId == evt.Id);
            var memberCount = await _db.EventMemberships.CountAsync(x => x.EventId == evt.Id, ct);
            var coverDto = await GetCoverPhotoDtoAsync(evt.CoverPhotoMediaFileId, ct);

            items.Add(new MyEventItem(
                EventId: evt.Id,
                Code: evt.Code,
                Title: evt.Title,
                Date: evt.Date,
                Status: evt.Status,
                MyRole: membership?.Role ?? EventRole.Participant,
                ParticipantLimit: evt.Specs.ParticipantLimit,
                MemberCount: memberCount,
                CoverPhoto: coverDto
            ));
        }

        return items.OrderByDescending(x => x.Date).ToList();
    }

    public async Task<IReadOnlyList<MyEventItem>> GetAllEventsPagedAsync(
        int page,
        int pageSize,
        CancellationToken ct)
    {
        var skip = (page - 1) * pageSize;

        var events = await _db.Events
            .AsNoTracking()
            .OrderByDescending(e => e.CreatedAtUtc)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = new List<MyEventItem>();
        foreach (var evt in events)
        {
            var memberCount = await _db.EventMemberships.CountAsync(x => x.EventId == evt.Id, ct);
            var coverDto = await GetCoverPhotoDtoAsync(evt.CoverPhotoMediaFileId, ct);

            items.Add(new MyEventItem(
                EventId: evt.Id,
                Code: evt.Code,
                Title: evt.Title,
                Date: evt.Date,
                Status: evt.Status,
                MyRole: EventRole.Admin,
                ParticipantLimit: evt.Specs.ParticipantLimit,
                MemberCount: memberCount,
                CoverPhoto: coverDto
            ));
        }

        return items;
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

        var eventData = await _db.Events
            .AsNoTracking()
            .Where(e => e.Id == eventId)
            .Select(e => new
            {
                e.Id,
                e.Code,
                e.Title,
                e.Description,
                e.Date,
                e.Status,
                e.CreatedByUserId,
                e.CoverPhotoMediaFileId,
                e.Specs.ParticipantLimit,
                e.Specs.PhotosPerUserLimit,
                e.Specs.VideosPerUserLimit,
                MemberCount = _db.EventMemberships.Count(m => m.EventId == e.Id),
                HasActiveInvite = role == EventRole.Admin
                    ? _db.Set<EventInvite>().Any(i => i.EventId == e.Id && i.IsActive)
                    : false
            })
            .FirstOrDefaultAsync(ct);

        if (eventData == null)
            return null;

        var coverDto = await GetCoverPhotoDtoAsync(eventData.CoverPhotoMediaFileId, ct);

        return new EventDetailsDto(
            eventData.Id,
            eventData.Code,
            eventData.Title,
            eventData.Description,
            eventData.Date,
            eventData.Status,
            eventData.CreatedByUserId,
            coverDto,
            role,
            eventData.ParticipantLimit,
            eventData.PhotosPerUserLimit,
            eventData.VideosPerUserLimit,
            eventData.MemberCount,
            eventData.HasActiveInvite
        );
    }

    public async Task<EventDetailsDto?> GetEventDetailsByIdAsync(Guid eventId, CancellationToken ct)
    {
        var eventData = await _db.Events
            .AsNoTracking()
            .Where(e => e.Id == eventId)
            .Select(e => new
            {
                e.Id,
                e.Code,
                e.Title,
                e.Description,
                e.Date,
                e.Status,
                e.CreatedByUserId,
                e.CoverPhotoMediaFileId,
                e.Specs.ParticipantLimit,
                e.Specs.PhotosPerUserLimit,
                e.Specs.VideosPerUserLimit,
                MemberCount = _db.EventMemberships.Count(m => m.EventId == e.Id),
                HasActiveInvite = _db.Set<EventInvite>().Any(i => i.EventId == e.Id && i.IsActive)
            })
            .FirstOrDefaultAsync(ct);

        if (eventData == null)
            return null;

        var coverDto = await GetCoverPhotoDtoAsync(eventData.CoverPhotoMediaFileId, ct);

        return new EventDetailsDto(
            eventData.Id,
            eventData.Code,
            eventData.Title,
            eventData.Description,
            eventData.Date,
            eventData.Status,
            eventData.CreatedByUserId,
            coverDto,
            EventRole.Admin,
            eventData.ParticipantLimit,
            eventData.PhotosPerUserLimit,
            eventData.VideosPerUserLimit,
            eventData.MemberCount,
            eventData.HasActiveInvite
        );
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
