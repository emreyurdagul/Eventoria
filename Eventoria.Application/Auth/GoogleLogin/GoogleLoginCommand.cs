using Eventoria.Application.Auth.Contracts;
using MediatR;

namespace Eventoria.Application.Auth.GoogleLogin;

public sealed record GoogleLoginCommand(
    string ProviderKey,
    string Email,
    string? FirstName,
    string? LastName
) : IRequest<AuthResponse>;
