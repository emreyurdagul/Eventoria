using Eventoria.Application.Abstractions.Persistence;
using Eventoria.Application.Abstractions.Security;
using Eventoria.Domain.Entities;
using Eventoria.Domain.Enums;
using MediatR;

namespace Eventoria.Application.Events.Create;

public sealed class CreateEventHandler : IRequestHandler<CreateEventCommand, CreateEventResult>
{
    private readonly IEventRepository _events;
    private readonly IUnitOfWork _uow;
    private readonly IEventTokenService _tokens;

    public CreateEventHandler(IEventRepository events, IUnitOfWork uow, IEventTokenService tokens)
    {
        _events = events;
        _uow = uow;
        _tokens = tokens;
    }

    public async Task<CreateEventResult> Handle(CreateEventCommand cmd, CancellationToken ct)
    {
        if (cmd.CreatorUserId == Guid.Empty)
            throw new InvalidOperationException("CreatorUserId is required.");

        if (string.IsNullOrWhiteSpace(cmd.Title))
            throw new InvalidOperationException("Title is required.");

        if (cmd.ParticipantLimit <= 0)
            throw new InvalidOperationException("ParticipantLimit must be > 0.");

        if (cmd.PhotosPerUserLimit < 0 || cmd.VideosPerUserLimit < 0)
            throw new InvalidOperationException("Media limits must be >= 0.");

        // Code retry
        string code;
        var tries = 0;
        do
        {
            if (++tries > 10)
                throw new InvalidOperationException("Could not generate unique event code.");
            code = _tokens.GenerateEventCode(8);
        } while (await _events.CodeExistsAsync(code, ct));

        // InviteKey plain -> hash
        var inviteKey = _tokens.GenerateInviteKey(32);
        var inviteHash = _tokens.Sha256Hex(inviteKey);

        var specs = new EventSpecs(cmd.ParticipantLimit, cmd.PhotosPerUserLimit, cmd.VideosPerUserLimit);

        var ev = new Event(cmd.Title.Trim(), cmd.Description?.Trim(), cmd.Date, cmd.CreatorUserId, code, specs);
        ev.AddMembership(cmd.CreatorUserId, EventRole.Admin);
        ev.AddInvite(inviteHash);

        await _events.AddAsync(ev, ct);

        // Save (ister behavior ile transaction yaparız, ister burada bırakırız)
        await _uow.SaveChangesAsync(ct);

        return new CreateEventResult(ev.Id, ev.Code, inviteKey);
    }
}
