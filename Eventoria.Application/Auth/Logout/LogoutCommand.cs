using Eventoria.Application.Auth.Contracts;
using MediatR;

namespace Eventoria.Application.Auth.Logout;

public sealed record LogoutCommand(LogoutRequest Request) : IRequest;
