using Eventoria.Application.Auth.Contracts;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Eventoria.Application.Auth.UpgradeGuest
{
    public sealed record UpgradeCommand(Guid CurrentUserId, string Email, string Password) : IRequest<AuthResponse>;

}
