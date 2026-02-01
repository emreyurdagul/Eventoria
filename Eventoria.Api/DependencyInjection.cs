using Eventoria.Application.Abstractions.Persistence;
using Eventoria.Application.Abstractions.Security;
using Eventoria.Infrastructure.Persistence;
using Eventoria.Infrastructure.Security;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

            // v10: requirement document üzerinden reference ile veriliyor
            o.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
            });
        });
        // JWT
        var key = config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key missing");
        var issuer = config["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer missing");
        var audience = config["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience missing");

        services
          .AddAuthentication(options =>
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
          });

        // MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(
                typeof(Eventoria.Application.Events.Create.CreateEventHandler).Assembly);
        });

        // Temporary registrations (idealde Infrastructure'da olur)
        services.AddScoped<IEventAdminQuotaRepository, EventAdminQuotaRepository>();
        services.AddSingleton<IEventTokenService, EventTokenService>();

        services.AddAuthorization();

        return services;
    }
}
