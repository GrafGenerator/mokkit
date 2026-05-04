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

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

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