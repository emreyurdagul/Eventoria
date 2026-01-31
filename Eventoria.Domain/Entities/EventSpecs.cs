namespace Eventoria.Domain.Entities;

public sealed record EventSpecs
{
    public int ParticipantLimit { get; init; }
    public int PhotosPerUserLimit { get; init; }
    public int VideosPerUserLimit { get; init; }

    private EventSpecs() { } // EF

    public EventSpecs(int participantLimit, int photosPerUserLimit, int videosPerUserLimit)
    {
        if (participantLimit <= 0) throw new ArgumentOutOfRangeException(nameof(participantLimit));
        if (photosPerUserLimit < 0) throw new ArgumentOutOfRangeException(nameof(photosPerUserLimit));
        if (videosPerUserLimit < 0) throw new ArgumentOutOfRangeException(nameof(videosPerUserLimit));

        ParticipantLimit = participantLimit;
        PhotosPerUserLimit = photosPerUserLimit;
        VideosPerUserLimit = videosPerUserLimit;
    }
}
