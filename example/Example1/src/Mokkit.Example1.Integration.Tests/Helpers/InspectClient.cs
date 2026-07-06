using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Mokkit.Example1.Application.Logic.Messages;
using Mokkit.Example1.Application.Logic.Persistence;
using Mokkit.Example1.Db.Postgres;
using Mokkit.Example1.Domain.Entities;
using Mokkit.Example1.Integration.Tests.Helpers;
using Mokkit.Inspect;
using Moq;
using VerifyNUnit;
using Capture = Mokkit.Capture;

namespace Mokkit.Example1.Integration.Tests;

/// <summary>
/// Cross-feature inspect building blocks shared by all client tests: real database-state
/// assertions, cache / Kafka mock-interaction verification, and snapshot helpers.
/// </summary>
public static class InspectClient
{
    /// <summary>
    /// Reads the client with the given id from the real database, captures it for snapshotting,
    /// and runs an optional inline assertion against it.
    /// </summary>
    public static ITestInspect DbClientById(
        this ITestInspect inspect,
        Guid clientId,
        out Capture<Client?> clientCapture,
        Action<Client?>? assert = null)
    {
        var capture = Capture.Start(out clientCapture);

        return inspect.Then(async host =>
        {
            await host.ExecuteAsync<ExampleContext>(async context =>
            {
                var client = await context.Clients
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == clientId);

                capture.Set(client);
                assert?.Invoke(client);
            });
        });
    }

    /// <summary>Asserts the clients table is empty.</summary>
    public static ITestInspect NoClientsInDb(this ITestInspect inspect)
    {
        return inspect.Then(async host =>
        {
            await host.ExecuteAsync<ExampleContext>(async context =>
            {
                var count = await context.Clients.AsNoTracking().CountAsync();
                Assert.That(count, Is.Zero, "Expected no clients to be persisted");
            });
        });
    }

    /// <summary>Verifies a client event was published to Kafka for the given id and type.</summary>
    public static ITestInspect EventPublished(
        this ITestInspect inspect,
        Guid clientId,
        string eventType,
        Times? times = null)
    {
        return inspect.Then(host =>
        {
            host.Execute<Mock<IKafkaEventPublisher>>(mock =>
                mock.Verify(
                    x => x.PublishClientEventAsync(clientId, eventType, It.IsAny<CancellationToken>()),
                    times ?? Times.Once()));
        });
    }

    /// <summary>Verifies no Kafka events were published at all.</summary>
    public static ITestInspect NoEventsPublished(this ITestInspect inspect)
    {
        return inspect.Then(host =>
        {
            host.Execute<Mock<IKafkaEventPublisher>>(mock => mock.VerifyNoOtherCalls());
        });
    }

    /// <summary>Verifies the cache was refreshed with the client of the given id.</summary>
    public static ITestInspect CacheUpdated(this ITestInspect inspect, Guid clientId, Times? times = null)
    {
        return inspect.Then(host =>
        {
            host.Execute<Mock<IClientCacheService>>(mock =>
                mock.Verify(
                    x => x.SetClientAsync(It.Is<Client>(c => c.Id == clientId), It.IsAny<CancellationToken>()),
                    times ?? Times.Once()));
        });
    }

    /// <summary>Verifies the cache was never written to.</summary>
    public static ITestInspect CacheNotUpdated(this ITestInspect inspect)
    {
        return inspect.Then(host =>
        {
            host.Execute<Mock<IClientCacheService>>(mock =>
                mock.Verify(
                    x => x.SetClientAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()),
                    Times.Never()));
        });
    }

    /// <summary>Snapshots an arbitrary value with the project-wide Verify settings.</summary>
    public static ITestInspect Verify(
        this ITestInspect inspect,
        object? value,
        Action<VerifySettings>? configure = null,
        [CallerFilePath] string sourceFile = "")
    {
        return inspect.Then(async _ =>
        {
            await Verifier.Verify(value, VerifierSetup.Default(configure), sourceFile);
        });
    }

    /// <summary>Snapshots a captured value with the project-wide Verify settings.</summary>
    public static ITestInspect Verify<T>(
        this ITestInspect inspect,
        Capture<T> capture,
        Action<VerifySettings>? configure = null,
        [CallerFilePath] string sourceFile = "")
    {
        return inspect.Then(async _ =>
        {
            await Verifier.Verify(capture.Value, VerifierSetup.Default(configure), sourceFile);
        });
    }
}
