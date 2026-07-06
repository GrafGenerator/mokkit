namespace Mokkit.Example1.Unit.Tests.Containers;

/// <summary>
/// Registration surface used while configuring the substitute container — the NSubstitute
/// analogue of Mokkit's Moq <c>IMockCollection</c>.
/// </summary>
public interface ISubstituteCollection
{
    /// <summary>Registers a substitute for service interface <typeparamref name="T"/>.</summary>
    ISubstituteCollection AddSubstitute<T>() where T : class;

    /// <summary>The registered substitutes.</summary>
    IReadOnlyCollection<SubstituteRegistration> Registrations { get; }
}

/// <inheritdoc cref="ISubstituteCollection" />
public sealed class SubstituteCollection : ISubstituteCollection
{
    private readonly List<SubstituteRegistration> _registrations = [];
    private bool _isReadOnly;

    public ISubstituteCollection AddSubstitute<T>() where T : class
    {
        if (_isReadOnly)
        {
            throw new InvalidOperationException("Substitute collection is read-only.");
        }

        if (_registrations.Any(r => r.InnerType == typeof(T)))
        {
            throw new InvalidOperationException($"Substitute for type {typeof(T)} already added.");
        }

        _registrations.Add(new SubstituteRegistration(typeof(T), () => Substitute.For<T>()));
        return this;
    }

    public IReadOnlyCollection<SubstituteRegistration> Registrations => _registrations.AsReadOnly();

    internal void MakeReadOnly() => _isReadOnly = true;
}
