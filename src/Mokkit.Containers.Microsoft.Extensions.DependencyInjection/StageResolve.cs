using System;
using Mokkit.Suite;

namespace Mokkit.Containers.Microsoft.Extensions.DependencyInjection;

public class StageResolve : IStageResolve, IStageResolveSetup
{
    private TestHostBag? _bag;

    public object? Resolve(Type serviceType)
    {
        if (_bag == null)
        {
            throw new InvalidOperationException("Stage resolver is in corrupt state, test host bag is missing.");
        }

        return _bag.TryGet(serviceType);
    }

    public void SetBag(TestHostBag bag)
    {
        _bag = bag;
    }
}