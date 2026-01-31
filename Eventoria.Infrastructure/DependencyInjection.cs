using Eventoria.Application.Abstractions.Persistence;
using Eventoria.Application.Abstractions.Storage;
using Eventoria.Application.Auth.Abstractions;
using Eventoria.Infrastructure.Auth.Identity;
using Eventoria.Infrastructure.Auth.Jwt;
using Eventoria.Infrastructure.Auth.RefreshTokens;
using Eventoria.Infrastructure.Data;
using Eventoria.Infrastructure.Persistence;
using Eventoria.Infrastructure.Security;
using Eventoria.Infrastructure.Storage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Eventoria.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // DbContext
        services.AddDbContext<AppDbContext>(opt =>
            opt.UseNpgsql(config.GetConnectionString("Postgres")));

        // Persistence
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));

        // Auth - Clean Architecture (Application abstractions -> Infrastructure implementations)
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IJwtTokenService, JwtAccessTokenService>();
        services.AddScoped<IRefreshTokenStore, RefreshTokenStore>();
        services.Configure<StorageOptions>(config.GetSection(StorageOptions.SectionName));

        services.AddSingleton<IStorageProviderResolver, StorageProviderResolver>();

        services.AddScoped<IMediaFileRepository, MediaFileRepository>();
        // Identity (UserManager, RoleManager, SignInManager, token providers)
        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(opt =>
        {
            opt.User.RequireUniqueEmail = true;

            opt.Password.RequiredLength = 8;
            opt.Password.RequireDigit = true;
            opt.Password.RequireUppercase = false;
            opt.Password.RequireLowercase = false;
            opt.Password.RequireNonAlphanumeric = false;

            opt.Lockout.AllowedForNewUsers = true;
            opt.Lockout.MaxFailedAccessAttempts = 5;
            opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        return services;
    }
}
