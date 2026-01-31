using Eventoria.Application.Auth.Contracts;
using MediatR;

namespace Eventoria.Application.Auth.Refresh;

public sealed record RefreshCommand(RefreshRequest Request) : IRequest<AuthResponse>;
