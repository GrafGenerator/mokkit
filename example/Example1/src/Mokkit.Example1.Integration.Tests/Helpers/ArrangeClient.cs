using Mokkit.Arrange;
using Mokkit.Example1.Common;
using Mokkit.Example1.Db.Postgres;
using Mokkit.Example1.Domain.Entities;
using Moq;
using Capture = Mokkit.Capture;

namespace Mokkit.Example1.Integration.Tests;

/// <summary>
/// Cross-feature arrange building blocks shared by all client tests:
/// deterministic clock / id generation and seeding clients straight into the database.
/// </summary>
public static class ArrangeClient
{
    /// <summary>Fixed point in time used so timestamps are deterministic and assertable.</summary>
    public static readonly DateTime FixedUtcNow = new(2026, 1, 15, 9, 30, 0, DateTimeKind.Utc);

    /// <summary>Fixed identifier handed out by the arranged id generator.</summary>
    public static readonly Guid FixedClientId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public const string DefaultName = "Acme Corporation";
    public const string DefaultEmail = "contact@acme.test";
    public const string DefaultPhone = "+15555550100";
    public const int DefaultStatus = (int)ClientStatus.Active;

    /// <summary>
    /// Builds a client entity from the shared defaults, optionally mutated. Used both for
    /// database seeding and for arranging cache contents.
    /// </summary>
    public static Client NewDefaultClient(Action<Client>? mutate = null)
    {
        var client = new Client
        {
            Id = FixedClientId,
            Name = DefaultName,
            Email = DefaultEmail,
            Phone = DefaultPhone,
            Status = ClientStatus.Active,
            CreatedAt = FixedUtcNow,
            UpdatedAt = FixedUtcNow
        };

        mutate?.Invoke(client);

        return client;
    }

    /// <summary>
    /// Arranges the <see cref="IDateTimeProvider"/> mock to return a fixed UTC time.
    /// </summary>
    public static ITestArrange Clock(this ITestArrange arrange, DateTime? utcNow = null)
    {
        return arrange.Then(host =>
        {
            host.Execute<Mock<IDateTimeProvider>>(mock =>
                mock.SetupGet(x => x.UtcNow).Returns(utcNow ?? FixedUtcNow));
        });
    }

    /// <summary>
    /// Arranges the <see cref="IIdGenerator"/> mock to hand out the given ids in order
    /// (the last id is repeated once the queue is exhausted). Defaults to <see cref="FixedClientId"/>.
    /// </summary>
    public static ITestArrange Ids(this ITestArrange arrange, params Guid[] ids)
    {
        var sequence = ids.Length == 0 ? new[] { FixedClientId } : ids;

        return arrange.Then(host =>
        {
            host.Execute<Mock<IIdGenerator>>(mock =>
            {
                var queue = new Queue<Guid>(sequence);
                mock.Setup(x => x.NewId()).Returns(() => queue.Count > 1 ? queue.Dequeue() : queue.Peek());
            });
        });
    }

    /// <summary>
    /// Seeds an existing client straight into the database (bypassing the handler), so reads
    /// and updates have something to operate on. The created entity is captured for assertions.
    /// </summary>
    public static ITestArrange DbClient(
        this ITestArrange arrange,
        out Capture<Client> clientCapture,
        Action<Client>? mutate = null)
    {
        var capture = Capture.Start(out clientCapture);

        return arrange.Then(async host =>
        {
            await host.ExecuteAsync<ExampleContext>(async context =>
            {
                var client = NewDefaultClient(mutate);

                context.Clients.Add(client);
                await context.SaveChangesAsync();

                capture.Set(client);
            });
        });
    }
}
