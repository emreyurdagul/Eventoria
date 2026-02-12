using System;
using System.Collections.Generic;
using System.Text;

namespace Eventoria.Application.Abstractions.Auth
{
    public interface ICurrentUserService
    {
        Guid UserId { get; }          // yoksa Unauthorized fýrlatabilir
        Guid? UserIdOrNull { get; }   // yoksa null
        bool IsAuthenticated { get; }
        bool IsSuperAdmin { get; }
    }
}
