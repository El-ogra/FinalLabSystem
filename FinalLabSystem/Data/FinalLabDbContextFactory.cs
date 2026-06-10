using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace FinalLabSystem.Data;

public sealed class FinalLabDbContextFactory : IDesignTimeDbContextFactory<FinalLabDbContext>
{
    public FinalLabDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FinalLabDbContext>();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' not found in appsettings.json. "
                + "Ensure the file exists in the project root with a ConnectionStrings section.");

        optionsBuilder.UseSqlServer(connectionString);

        return new FinalLabDbContext(optionsBuilder.Options);
    }
}
