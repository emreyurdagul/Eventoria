using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Eventoria.Infrastructure.Data;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var apiPath = LocateApiDirectory();

        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var config = new ConfigurationBuilder()
            .SetBasePath(apiPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var cs = config.GetConnectionString("Postgres");
        if (string.IsNullOrWhiteSpace(cs))
            throw new InvalidOperationException(
                $"ConnectionStrings:Postgres missing. Loaded from: {Path.Combine(apiPath, "appsettings.json")} (ENV={env})");

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(cs)
            .Options;

        return new AppDbContext(options);
    }

    private static string LocateApiDirectory()
    {
        // dotnet ef komutunu nereden çalıştırırsan çalıştır, yukarı çıkarak appsettings.json ara.
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());

        for (var i = 0; i < 12 && dir != null; i++, dir = dir.Parent)
        {
            // 1) Standart: <root>/Eventoria.Api/appsettings.json
            var candidate1 = Path.Combine(dir.FullName, "Eventoria.Api", "appsettings.json");
            if (File.Exists(candidate1))
                return Path.GetDirectoryName(candidate1)!;

            // 2) Bazı projelerde: <root>/src/Eventoria.Api/appsettings.json
            var candidate2 = Path.Combine(dir.FullName, "src", "Eventoria.Api", "appsettings.json");
            if (File.Exists(candidate2))
                return Path.GetDirectoryName(candidate2)!;

            // 3) İç içe klasör: <root>/Eventoria/Eventoria.Api/appsettings.json (senin path buna benziyor)
            var candidate3 = Path.Combine(dir.FullName, "Eventoria", "Eventoria.Api", "appsettings.json");
            if (File.Exists(candidate3))
                return Path.GetDirectoryName(candidate3)!;
        }

        throw new DirectoryNotFoundException(
            "Could not locate Eventoria.Api/appsettings.json. Run dotnet ef from solution root, or adjust LocateApiDirectory().");
    }
}
