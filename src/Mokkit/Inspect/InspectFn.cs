using System.Threading.Tasks;
using Mokkit.Capture.Suite;

namespace Mokkit.Inspect;

public delegate void InspectFn(ITestHost host);

public delegate Task InspectAsyncFn(ITestHost host);