using System.Threading.Tasks;
using Mokkit.Suite;

namespace Mokkit.Arrange;

/// <summary>
/// Represents a synchronous arrange function that sets up test dependencies and state.
/// This delegate is used in the Arrange phase of the AAA (Arrange-Act-Assert) pattern.
/// </summary>
/// <param name="host">The test host that provides access to configured services and dependencies.</param>
public delegate void ArrangeFn(ITestHost host);

/// <summary>
/// Represents an asynchronous arrange function that sets up test dependencies and state.
/// This delegate is used in the Arrange phase of the AAA (Arrange-Act-Assert) pattern.
/// </summary>
/// <param name="host">The test host that provides access to configured services and dependencies.</param>
/// <returns>A task representing the asynchronous arrange operation.</returns>
public delegate Task ArrangeAsyncFn(ITestHost host);