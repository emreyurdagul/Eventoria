using Eventoria.Application.Auth.Abstractions;
using MediatR;

namespace Eventoria.Application.Auth.ForgotPassword;

public sealed class ForgotPasswordHandler : IRequestHandler<ForgotPasswordCommand, string>
{
    private readonly IIdentityService _identity;

    public ForgotPasswordHandler(IIdentityService identity)
    {
        _identity = identity;
    }

    public async Task<string> Handle(ForgotPasswordCommand cmd, CancellationToken ct)
    {
        var email = cmd.Request.Email.Trim().ToLowerInvariant();
        var token = await _identity.GeneratePasswordResetTokenAsync(email, ct);

        // security: kullanıcı yoksa da aynı cevap
        return token ?? "OK";
    }
}
