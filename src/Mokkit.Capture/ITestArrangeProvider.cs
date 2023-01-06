using System.Collections.Generic;

namespace Mokkit.Capture;

// TODO: internal
public interface ITestArrangeProvider
{
    IReadOnlyCollection<ArrangeAsyncFn> GetArrangeFunctions();
}