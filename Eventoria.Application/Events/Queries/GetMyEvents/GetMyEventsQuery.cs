using Eventoria.Application.Events.Queries.Models;
using MediatR;

namespace Eventoria.Application.Events.Queries.GetMyEvents;

public sealed record GetMyEventsQuery(Guid UserId) : IRequest<IReadOnlyList<MyEventItem>>;
