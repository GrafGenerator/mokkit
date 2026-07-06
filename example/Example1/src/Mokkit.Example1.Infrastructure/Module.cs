using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mokkit.Example1.Application.Logic.Messages;
using Mokkit.Example1.Application.Logic.Persistence;
using Mokkit.Example1.Infrastructure.Kafka.Consumers;
using Mokkit.Example1.Infrastructure.Logic.Cache;
using Mokkit.Example1.Infrastructure.Logic.Events;
using Mokkit.Example1.Infrastructure.Options;

namespace Mokkit.Example1.Infrastructure;

public static class Module
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services
            .RegisterCacheServices()
            .RegisterKafkaServices();

        return services;
    }

    private static IServiceCollection RegisterCacheServices(this IServiceCollection services)
    {
        services.AddScoped<IClientCacheService, ClientCacheService>();
        return services;
    }

    private static IServiceCollection RegisterKafkaServices(this IServiceCollection services)
    {
        services.AddSingleton<IProducer<string, string>>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<KafkaOptions>>().Value;
            var logger = sp.GetRequiredService<ILogger<IProducer<string, string>>>();
            var config = new ProducerConfig
            {
                BootstrapServers = options.BootstrapServers,
                ClientId = options.ClientId
            };

            return new ProducerBuilder<string, string>(config)
                .SetErrorHandler((_, e) => logger.LogError("Kafka producer error: {Error}", e.Reason))
                .Build();
        });

        services.AddSingleton<IConsumer<string, string>>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<KafkaOptions>>().Value;
            var logger = sp.GetRequiredService<ILogger<IConsumer<string, string>>>();
            var config = new ConsumerConfig
            {
                BootstrapServers = options.BootstrapServers,
                GroupId = options.ConsumerGroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            return new ConsumerBuilder<string, string>(config)
                .SetErrorHandler((_, e) => logger.LogError("Kafka consumer error: {Error}", e.Reason))
                .Build();
        });

        services.AddScoped<IKafkaEventPublisher, KafkaEventPublisher>();
        services.AddSingleton<IClientStatusChangedProcessor, ClientStatusChangedProcessor>();
        services.AddHostedService<ClientEventConsumer>();
        return services;
    }

    public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = connectionString;
            options.InstanceName = "Example1";
        });
        
        return services;
    }

    public static IServiceCollection AddKafkaOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KafkaOptions>(configuration.GetSection(KafkaOptions.SectionName));
        return services;
    }
}