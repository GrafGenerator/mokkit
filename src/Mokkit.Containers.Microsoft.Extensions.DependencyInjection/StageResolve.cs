using System;
using Mokkit.Suite;

namespace Mokkit.Containers.Microsoft.Extensions.DependencyInjection;

public class StageResolve : IStageResolve
{
    private readonly ITestHostBagAccessor _testHostBagAccessor;

    public StageResolve(ITestHostBagAccessor testHostBagAccessor)
    {
        _testHostBagAccessor = testHostBagAccessor;
    }
    
    public object? Resolve(Type serviceType)
    {
        var bag = _testHostBagAccessor.Bag;
        
        if (bag == null)
        {
            throw new InvalidOperationException("Stage resolver is in corrupt state, test host bag is missing.");
        }

        return bag.TryGet(serviceType);
    }
}