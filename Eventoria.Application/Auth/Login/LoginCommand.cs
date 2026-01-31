using Eventoria.Application.Auth.Contracts;
using MediatR;

namespace Eventoria.Application.Auth.Login;

public sealed record LoginCommand(LoginRequest Request) : IRequest<AuthResponse>;
