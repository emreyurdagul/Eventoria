using Eventoria.Application.Abstractions.Identity;
using MediatR;

namespace Eventoria.Application.Admin.EventAdmins.CreateOrAssign;

public sealed class CreateOrAssignEventAdminHandler
    : IRequestHandler<CreateOrAssignEventAdminCommand, CreateOrAssignEventAdminResult>
{
    private readonly IAdminIdentityService _identity;

    public CreateOrAssignEventAdminHandler(IAdminIdentityService identity)
    {
        _identity = identity;
    }

    public async Task<CreateOrAssignEventAdminResult> Handle(
        CreateOrAssignEventAdminCommand cmd,
        CancellationToken ct)
    {
        if (cmd.ActorUserId == Guid.Empty)
            throw new InvalidOperationException("ActorUserId is required.");

        if (string.IsNullOrWhiteSpace(cmd.Email))
            throw new InvalidOperationException("Email is required.");

        var email = cmd.Email.Trim().ToLowerInvariant();

        // ✅ Actor must be SuperUser (senin mevcut role ismin)
        var isSuperUser = await _identity.IsInRoleAsync(cmd.ActorUserId, "SuperUser", ct);
        if (!isSuperUser)
            throw new UnauthorizedAccessException("Only SuperUser can create/assign EventAdmin.");

        // ✅ Ensure EventAdmin role exists
        await _identity.EnsureRoleExistsAsync("EventAdmin", ct);

        var existing = await _identity.FindByEmailAsync(email, ct);

        bool created;
        UserSnapshot user;

        if (existing != null)
        {
            created = false;
            user = existing;
        }
        else
        {
            // temp password yoksa üret
            var pwd = string.IsNullOrWhiteSpace(cmd.TempPassword)
                ? $"Ev!{Guid.NewGuid():N}9"
                : cmd.TempPassword;

            user = await _identity.CreateUserAsync(email, pwd, ct);
            created = true;
        }

        await _identity.AddToRoleAsync(user.Id, "EventAdmin", ct);

        return new CreateOrAssignEventAdminResult(
            user.Id,
            user.Email,
            created,
            "EventAdmin"
        );
    }
}
