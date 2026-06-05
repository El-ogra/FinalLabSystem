using System.IO;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FinalLabSystem.Data;

public sealed class FinalLabDbContextFactory : IDesignTimeDbContextFactory<FinalLabDbContext>
{
    private const string FallbackConnectionString =
        "Server=.\\SQLEXPRESS;Database=FinalLab;Trusted_Connection=True;TrustServerCertificate=True;";

    public FinalLabDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FinalLabDbContext>();
        optionsBuilder.UseSqlServer(GetConnectionString());

        return new FinalLabDbContext(optionsBuilder.Options);
    }

    private static string GetConnectionString()
    {
        var appSettingsPath = FindAppSettingsPath();
        if (appSettingsPath is null)
            return FallbackConnectionString;

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(appSettingsPath));
            if (!document.RootElement.TryGetProperty("ConnectionStrings", out var connectionStrings))
                return FallbackConnectionString;

            return ReadConnectionString(connectionStrings, "DefaultConnection")
                ?? ReadConnectionString(connectionStrings, "FinalLabDbContext")
                ?? ReadConnectionString(connectionStrings, "FinalLab")
                ?? FallbackConnectionString;
        }
        catch (JsonException)
        {
            return FallbackConnectionString;
        }
        catch (IOException)
        {
            return FallbackConnectionString;
        }
        catch (UnauthorizedAccessException)
        {
            return FallbackConnectionString;
        }
    }

    private static string? FindAppSettingsPath()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var currentPath = Path.Combine(currentDirectory, "appsettings.json");
        if (File.Exists(currentPath))
            return currentPath;

        var baseDirectoryPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (File.Exists(baseDirectoryPath))
            return baseDirectoryPath;

        var projectDirectoryPath = Path.Combine(currentDirectory, "FinalLabSystem", "appsettings.json");
        return File.Exists(projectDirectoryPath) ? projectDirectoryPath : null;
    }

    private static string? ReadConnectionString(JsonElement connectionStrings, string name)
    {
        if (!connectionStrings.TryGetProperty(name, out var value))
            return null;

        var connectionString = value.GetString();
        return string.IsNullOrWhiteSpace(connectionString) ? null : connectionString;
    }
}
