using Mokkit.Suite;

namespace Mokkit.Containers;

/// <summary>
/// Represents a dependency injection container that provides scoped service resolution.
/// This interface abstracts the underlying dependency injection implementation and allows for pluggable container architectures.
/// </summary>
public interface IDependencyContainer
{
    /// <summary>
    /// Creates a new dependency injection scope within the specified test host context.
    /// The scope provides isolated service resolution for the duration of a test operation.
    /// </summary>
    /// <param name="context">The test host context that defines the scope parameters.</param>
    /// <returns>A new <see cref="IDependencyContainerScope"/> for scoped service resolution.</returns>
    IDependencyContainerScope BeginScope(TestHostContext context);
}