using Eventoria.Application.Auth;
using Eventoria.Infrastructure.Security;
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

        // 🔥 Swagger'ı en sade ve stabil haliyle ekliyoruz
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Eventoria API",
                Version = "v1",
                Description = "Eventoria – Event & Media Platform API",
                Contact = new OpenApiContact
                {
                    Name = "Eventoria Team",
                    Email = "dev@eventoria.app"
                }
            });

            // XML comments (docstring)
            var xmlFile = $"{typeof(Program).Assembly.GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

            // XML dosyası yoksa ya da bozuksa uygulamayı patlatma
            if (File.Exists(xmlPath))
            {
                try
                {
                    // Boş dosya mı kontrol (0 byte)
                    var fi = new FileInfo(xmlPath);
                    if (fi.Length > 0)
                        c.IncludeXmlComments(xmlPath);
                }
                catch
                {
                    // Swagger’ı düşürme. Loglamak istersen buraya logger ekleriz.
                }
            }

            // JWT Auth
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Token giriniz: Bearer {token}"
            });

        });


        // JWT
        var key = config["Jwt:Key"]!;
        var issuer = config["Jwt:Issuer"]!;
        var audience = config["Jwt:Audience"]!;

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
                };
            });

        services.AddAuthorization();


        return services;
    }
}
