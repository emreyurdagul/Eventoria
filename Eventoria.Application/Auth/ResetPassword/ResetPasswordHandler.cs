using Eventoria.Application.Auth.Abstractions;
using MediatR;

namespace Eventoria.Application.Auth.ResetPassword;

public sealed class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand>
{
    private readonly IIdentityService _identity;

    public ResetPasswordHandler(IIdentityService identity)
    {
        _identity = identity;
    }

    public async Task Handle(ResetPasswordCommand cmd, CancellationToken ct)
    {
        var email = cmd.Request.Email.Trim().ToLowerInvariant();
        var (ok, err) = await _identity.ResetPasswordAsync(email, cmd.Request.Token, cmd.Request.NewPassword, ct);

        if (!ok)
            throw new InvalidOperationException(err ?? "Invalid request.");
    }
}
