using Eventoria.Application.Auth.Contracts;
using MediatR;

namespace Eventoria.Application.Auth.Register;

public sealed record RegisterCommand(RegisterRequest Request) : IRequest<AuthResponse>;
