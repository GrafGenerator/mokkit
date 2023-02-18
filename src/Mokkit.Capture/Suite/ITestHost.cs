using System;
using System.Threading.Tasks;

namespace Mokkit.Capture.Suite;

public interface ITestHost
{
    Task ExecuteScopeAsync<TService>(Action<TService> actionFn);
}