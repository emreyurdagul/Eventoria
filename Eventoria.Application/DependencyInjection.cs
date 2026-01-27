using Microsoft.Extensions.DependencyInjection;

namespace Eventoria.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}
