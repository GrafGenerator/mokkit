using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Mokkit.Example1.Db.Postgres.Options;

namespace Mokkit.Example1.Db.Postgres;

public static class Module
{
    public static IServiceCollection AddPostgresDbContext(this IServiceCollection services)
    {
        services.AddDbContext<ExampleContext>((sp, builder) =>
        {
            var connectionStringsOptions = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            BuildContextWithOptions(builder, connectionStringsOptions);
        });

        return services;
    }

    public static IServiceCollection AddDataLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DatabaseOptions>(options => 
            configuration.GetSection(DatabaseOptions.SectionName).Bind(options));
        services.AddPostgresDbContext();

        return services;
    }

    internal static void BuildContextWithOptions(
        DbContextOptionsBuilder optionsBuilder,
        DatabaseOptions databaseOptions)
    {
        optionsBuilder.UseNpgsql(databaseOptions.Primary,
            npgsqlOptionsBuilder => npgsqlOptionsBuilder.MigrationsAssembly(typeof(ExampleContext).Assembly.FullName));
    }
}