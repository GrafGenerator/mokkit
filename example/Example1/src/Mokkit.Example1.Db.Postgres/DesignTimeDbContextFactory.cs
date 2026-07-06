using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Mokkit.Example1.Db.Postgres.Options;

namespace Mokkit.Example1.Db.Postgres;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ExampleContext>
{
    public ExampleContext CreateDbContext(string[] args)
    {
        var appRootPath = Directory.GetCurrentDirectory();
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        Console.WriteLine($"[=== Design Time Context ===]: loading config from path '{appRootPath}'");
        
        var builder = new ConfigurationBuilder()
                .SetBasePath(appRootPath)
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environmentName}.json");

        var config = builder.Build();

        var databaseOptions = new DatabaseOptions();
        config.GetSection(DatabaseOptions.SectionName).Bind(databaseOptions);

        var connectionString = databaseOptions.Primary;
       
        Console.WriteLine($"[=== Design Time Context ===]: using connection string = {connectionString}");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException($"[=== Design Time Context ===]: no connection string found in '{DatabaseOptions.SectionName}'");
        }
        
        var optionsBuilder = new DbContextOptionsBuilder<ExampleContext>();

        Module.BuildContextWithOptions(optionsBuilder, databaseOptions);

        return new ExampleContext(optionsBuilder.Options);
    }
}