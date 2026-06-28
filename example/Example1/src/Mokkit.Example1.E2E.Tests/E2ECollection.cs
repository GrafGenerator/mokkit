namespace Mokkit.Example1.E2E.Tests;

/// <summary>
/// Shares the (expensive) Docker stack across every E2E test class — built once for the whole run.
/// </summary>
[CollectionDefinition(Name)]
public sealed class E2ECollection : ICollectionFixture<E2EStack>
{
    public const string Name = "e2e";
}
