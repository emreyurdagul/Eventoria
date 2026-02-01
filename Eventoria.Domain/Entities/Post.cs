using Eventoria.Domain.Common;

namespace Eventoria.Domain.Entities;

public sealed class Post : AggregateRoot
{
    private readonly List<PostMedia> _media = [];

    private Post() { } // EF

    public Post(Guid eventId, Guid createdByUserId, string? caption)
    {
        if (eventId == Guid.Empty) throw new ArgumentException("EventId required");
        if (createdByUserId == Guid.Empty) throw new ArgumentException("CreatedByUserId required");

        Id = Guid.NewGuid();
        EventId = eventId;
        CreatedByUserId = createdByUserId;
        Caption = caption?.Trim();
    }

    public Guid EventId { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public string? Caption { get; private set; }

    public IReadOnlyCollection<PostMedia> Media => _media;

    public void UpdateCaption(string? caption)
    {
        Caption = caption?.Trim();
        SetUpdated();
    }

    public void AddMedia(Guid mediaFileId)
    {
        if (mediaFileId == Guid.Empty)
            throw new ArgumentException("MediaFileId required");

        if (_media.Any(x => x.MediaFileId == mediaFileId))
            return;

        var nextOrder = _media.Count == 0
            ? 0
            : _media.Max(x => x.Order) + 1;

        _media.Add(new PostMedia(Id, mediaFileId, nextOrder));
        SetUpdated();
    }


    public bool RemoveMedia(Guid mediaFileId)
    {
        var pm = _media.FirstOrDefault(x => x.MediaFileId == mediaFileId);
        if (pm == null) return false;

        _media.Remove(pm);
        SetUpdated(); // BaseEntity'deki timestamp + concurrency
        return true;
    }

    public void NormalizeOrder()
    {
        // mevcut sıraya göre tekrar 0..n-1 bas
        var ordered = _media.OrderBy(x => x.Order).ToList();
        for (var i = 0; i < ordered.Count; i++)
            ordered[i].SetOrder(i);

        SetUpdated();
    }
    public void SetMediaOrder(IReadOnlyList<Guid> orderedMediaFileIds)
    {
        if (orderedMediaFileIds == null || orderedMediaFileIds.Count == 0)
            throw new InvalidOperationException("Media order list is empty.");

        // mevcut set ile aynı mı?
        var existing = _media.Select(x => x.MediaFileId).ToHashSet();
        var incoming = orderedMediaFileIds.ToHashSet();

        if (!existing.SetEquals(incoming))
            throw new InvalidOperationException("Order list must contain exactly the existing media items.");

        for (int i = 0; i < orderedMediaFileIds.Count; i++)
        {
            var mfId = orderedMediaFileIds[i];
            var pm = _media.First(x => x.MediaFileId == mfId);
            pm.SetOrder(i);
        }

        NormalizeOrder();
        SetUpdated();
    }


}
