using System.Threading.Tasks;
using Mokkit.Suite;

namespace Mokkit.Arrange;

public delegate void ArrangeFn(ITestHost host);

public delegate Task ArrangeAsyncFn(ITestHost host);