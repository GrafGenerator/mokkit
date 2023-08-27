using Mokkit.Suite;

namespace Mokkit.Containers.Microsoft.Extensions.DependencyInjection;

public interface IStageResolveSetup
{
    void SetBag(TestHostBag bag);
}