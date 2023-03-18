using System.Threading.Tasks;
using Mokkit.Capture.Suite;

namespace Mokkit.Capture.Arrange;

public delegate void ArrangeFn(ITestHost host);

public delegate Task ArrangeAsyncFn(ITestHost host);