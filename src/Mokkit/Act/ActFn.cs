using System.Threading.Tasks;
using Mokkit.Suite;

namespace Mokkit.Act;

/// <summary>
/// Represents a synchronous act function that performs the operation under test.
/// This delegate is used in the Act phase of the AAA (Arrange-Act-Assert) pattern.
/// </summary>
/// <param name="host">The test host that provides access to configured services and dependencies.</param>
public delegate void ActFn(ITestHost host);

/// <summary>
/// Represents an asynchronous act function that performs the operation under test.
/// This delegate is used in the Act phase of the AAA (Arrange-Act-Assert) pattern.
/// </summary>
/// <param name="host">The test host that provides access to configured services and dependencies.</param>
/// <returns>A task representing the asynchronous act operation.</returns>
public delegate Task ActAsyncFn(ITestHost host);
