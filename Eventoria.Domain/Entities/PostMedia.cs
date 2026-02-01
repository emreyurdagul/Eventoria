using Eventoria.Domain.Common;

namespace Eventoria.Domain.Entities;

public sealed class PostMedia : BaseEntity
{
    private PostMedia() { } // EF

    internal PostMedia(Guid postId, Guid mediaFileId, int order)
    {
        if (postId == Guid.Empty) throw new ArgumentException("PostId required");
        if (mediaFileId == Guid.Empty) throw new ArgumentException("MediaFileId required");
        if (order < 0) throw new ArgumentOutOfRangeException(nameof(order));

        Id = Guid.NewGuid();
        PostId = postId;
        MediaFileId = mediaFileId;
        Order = order;
    }

    public Guid PostId { get; private set; }
    public Guid MediaFileId { get; private set; }
    public int Order { get; private set; }

    internal void SetOrder(int order)
    {
        if (order < 0) throw new ArgumentOutOfRangeException(nameof(order));
        Order = order;
        SetUpdated();
    }
}
