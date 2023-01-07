using System;
using System.Threading.Tasks;

namespace Mokkit.Capture.Suite;

public class TestStage
{

    public ITestArrange Arrange()
    {
        return Mokkit.Capture.Arrange.Start(this);
    }

    private async Task ArrangeAsync(ITestArrange arrange)
    {
        if (arrange is not ITestArrangeProvider provider)
        {
            throw new InvalidOperationException("Specified test arrange has no provider implementation");
        }

        var arrangeFns = provider.GetArrangeFunctions();

        foreach (var arrangeFn in arrangeFns)
        {
            await arrangeFn();
        }
    }
}