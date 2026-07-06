using Microsoft.EntityFrameworkCore;
using Mokkit.Example1.Db.Postgres.Configurations;
using Mokkit.Example1.Domain.Entities;

namespace Mokkit.Example1.Db.Postgres;

public sealed class ExampleContext : DbContext
{
    public DbSet<Client> Clients { get; private set; }

    public ExampleContext(DbContextOptions<ExampleContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(DatabaseConstants.ServiceSchemaName);
        modelBuilder.ApplyConfiguration(new ClientEntityConfiguration());
    }
}