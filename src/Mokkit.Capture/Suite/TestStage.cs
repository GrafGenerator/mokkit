using System;
using System.Threading.Tasks;

namespace Mokkit.Capture.Suite;

public class TestStage: ITestHost
{
    public ITestArrange Arrange()
    {
        return Mokkit.Capture.Arrange.Start(this);
    }

    public Task ExecuteScopeAsync<TService>(Action<TService> actionFn)
    {
        throw new NotImplementedException();
    }
}