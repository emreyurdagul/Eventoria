using Eventoria.Application.Auth.Contracts;
using MediatR;

namespace Eventoria.Application.Auth.ForgotPassword;

public sealed record ForgotPasswordCommand(ForgotPasswordRequest Request) : IRequest<string>;
