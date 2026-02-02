using Eventoria.Api;
using Eventoria.Application;
using Eventoria.Infrastructure;
using Eventoria.Infrastructure.Data;
using Eventoria.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddFilter("Microsoft.AspNetCore.Authentication", LogLevel.Debug);
builder.Logging.AddFilter("Microsoft.IdentityModel", LogLevel.Debug);

builder.Services
    .AddApi(builder.Configuration)
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var pendingMigrations = await db.Database.GetPendingMigrationsAsync();
    if (pendingMigrations.Any())
    {
        await db.Database.MigrateAsync();
    }
}


await SuperUserSeeder.SeedAsync(app.Services);
await RoleSeeder.SeedAsync(app.Services);


app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Eventoria.Api v1");
});



app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
