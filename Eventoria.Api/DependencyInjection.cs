using Eventoria.Api.Middleware;
using Eventoria.Application.Abstractions.Persistence;
using Eventoria.Application.Abstractions.Security;
using Eventoria.Application.Events.Create;
using Eventoria.Infrastructure.Persistence;
using Eventoria.Infrastructure.Security;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

namespace Eventoria.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration config)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(o =>
        {
            o.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Eventoria API",
                Version = "v1",
                Description = "Eventoria – Event & Media Platform API"
            });

            o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Token giriniz: Bearer {token}"
            });

            o.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
            });
        });

        // JWT
        var key = config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key missing");
        var issuer = config["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer missing");
        var audience = config["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience missing");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(opt =>
        {
            opt.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ClockSkew = TimeSpan.FromMinutes(2)
            };
        })
        // ✅ External cookie scheme ismi IdentityConstants.ExternalScheme ile aynı olmalı
        .AddCookie(IdentityConstants.ExternalScheme, opt =>
        {
            opt.Cookie.Name = "eventoria.external";
            opt.ExpireTimeSpan = TimeSpan.FromMinutes(5);
            opt.Cookie.SameSite = SameSiteMode.None;
            opt.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            opt.Cookie.HttpOnly = true;
        })
        .AddGoogle("Google", opt =>
        {
            opt.ClientId = config["Authentication:Google:ClientId"]
                ?? throw new InvalidOperationException("Google ClientId missing");
            opt.ClientSecret = config["Authentication:Google:ClientSecret"]
                ?? throw new InvalidOperationException("Google ClientSecret missing");

            // ✅ Google login sonucu external cookie'ye yazılacak
            opt.SignInScheme = IdentityConstants.ExternalScheme;

            // ✅ Google'ın döneceği middleware endpoint
            opt.CallbackPath = "/signin-google";

            opt.Scope.Add("email");
            opt.Scope.Add("profile");

            opt.CorrelationCookie.SameSite = SameSiteMode.None;
            opt.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
        });

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(CreateEventHandler).Assembly);
        });

        services.AddScoped<IEventAdminQuotaRepository, EventAdminQuotaRepository>();
        services.AddSingleton<IEventTokenService, EventTokenService>();
        services.AddScoped<ExceptionHandlingMiddleware>();

        services.AddAuthorization();

        return services;
    }
}
