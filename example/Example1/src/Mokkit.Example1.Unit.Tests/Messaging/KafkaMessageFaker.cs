using System.Text.Json;
using Bogus;
using Mokkit.Example1.Domain.Entities;
using Mokkit.Example1.Infrastructure.Kafka.Consumers;

namespace Mokkit.Example1.Unit.Tests.Messaging;

/// <summary>
/// Generates <see cref="ClientStatusChangedMessage"/> payloads (and their JSON) with Bogus, fixed seed.
/// </summary>
internal static class KafkaMessageFaker
{
    private static readonly Faker Faker = new() { Random = new Randomizer(98765) };

    public static ClientStatusChangedMessage NewStatusChanged(Guid? clientId = null, int? status = null) =>
        new()
        {
            ClientId = clientId ?? Faker.Random.Guid(),
            Name = Faker.Company.CompanyName(),
            Email = Faker.Internet.Email(),
            Phone = Faker.Phone.PhoneNumber("+1##########"),
            Status = status ?? (int)ClientStatus.Suspended
        };

    public static string ToJson(ClientStatusChangedMessage message) => JsonSerializer.Serialize(message);
}
