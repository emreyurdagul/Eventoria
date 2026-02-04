using System;
using System.Collections.Generic;
using System.Text;

namespace Eventoria.Application.Auth.UpgradeGuest
{
    public sealed record UpgradeGuestRequest(string Email, string Password);

}
