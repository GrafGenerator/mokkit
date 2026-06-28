using Microsoft.EntityFrameworkCore;
using Mokkit.Example1.Api;
using Mokkit.Example1.Api.Routes;
using Mokkit.Example1.Application;
using Mokkit.Example1.Db.Postgres;
using Mokkit.Example1.Infrastructure;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container
    builder.Services
        .AddEndpointsApiExplorer()
        .AddSwaggerGen()
        .AddDataLayer(builder.Configuration)
        .AddApplicationLayer()
        .AddInfrastructure()
        .AddRedisCache(builder.Configuration)
        .AddKafkaOptions(builder.Configuration)
        .AddExceptionHandler<ExceptionHandler>();

    var app = builder.Build();

    // Provision the database schema (apply EF migrations) before serving traffic.
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ExampleContext>();
        await dbContext.Database.MigrateAsync();
    }

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Liveness/readiness probe used by container orchestration and E2E wait strategies.
    app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

    app.MapApiRoutes();
    app.UseExceptionHandler(_ => { });

    app.Logger.LogInformation("Starting Example1 Client Service...");
    app.Logger.LogInformation("Swagger UI available at: /swagger");
    app.Logger.LogInformation("Client API available at: /api/v1/clients");
    
    await app.RunAsync();
    return 0;
}
catch (Exception e)
{
    Console.WriteLine($"Unable to start host: {e.Message}");
    Console.WriteLine($"Stack trace: {e.StackTrace}");
    return 1;
}