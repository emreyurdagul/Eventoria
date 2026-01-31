using Eventoria.Application.Events.Queries.Models;
using MediatR;

namespace Eventoria.Application.Events.Queries.GetEventDetails;

public sealed record GetEventDetailsQuery(Guid UserId, Guid EventId) : IRequest<EventDetailsDto>;
