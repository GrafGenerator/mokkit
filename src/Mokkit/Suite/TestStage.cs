using System;
using System.Collections.Generic;
using Mokkit.Arrange;
using Mokkit.Containers;
using Mokkit.Inspect;

namespace Mokkit.Suite;

public class TestStage : TestHost
{
    public TestStage(IEnumerable<IDependencyContainer> containers, ITestHostBagAccessor bagAccessor, Guid stageId)
        : base(containers, bagAccessor, stageId)
    {
    }

    public ITestArrange Arrange()
    {
        return Mokkit.Arrange.Arrange.Start(this);
    }

    public ITestInspect Inspect()
    {
        return Mokkit.Inspect.Inspect.Start(this);
    }
}