using Bogus;
using Mokkit.Example1.Domain.Entities;

namespace Mokkit.Example1.Unit.Tests;

/// <summary>
/// Generates realistic <see cref="Client"/> data with Bogus. A fixed seed keeps generation
/// deterministic so tests are reproducible; callers can pin specific fields via <paramref name="mutate"/>.
/// </summary>
public static class ClientFaker
{
    public static readonly DateTime FixedUtcNow = new(2026, 1, 15, 9, 30, 0, DateTimeKind.Utc);

    private static readonly Faker<Client> Faker = new Faker<Client>()
        .UseSeed(20260115)
        .RuleFor(c => c.Id, f => f.Random.Guid())
        .RuleFor(c => c.Name, f => f.Company.CompanyName())
        .RuleFor(c => c.Email, f => f.Internet.Email())
        .RuleFor(c => c.Phone, f => f.Phone.PhoneNumber("+1##########"))
        .RuleFor(c => c.Status, ClientStatus.Active)
        .RuleFor(c => c.CreatedAt, FixedUtcNow)
        .RuleFor(c => c.UpdatedAt, FixedUtcNow);

    public static Client NewClient(Action<Client>? mutate = null)
    {
        var client = Faker.Generate();
        mutate?.Invoke(client);
        return client;
    }
}
