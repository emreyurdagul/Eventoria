using Eventoria.Application.Auth.Contracts;
using MediatR;

namespace Eventoria.Application.Auth.ResetPassword;

public sealed record ResetPasswordCommand(ResetPasswordRequest Request) : IRequest;
