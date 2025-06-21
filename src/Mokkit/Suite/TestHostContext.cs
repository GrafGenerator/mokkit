using System;

namespace Mokkit.Suite;

public class TestHostContext
{
    public TestHostContext(Guid testHostId, TestHostBag bag)
    {
        TestHostId = testHostId;
        Bag = bag;
    }

    public Guid TestHostId { get; }

    public TestHostBag Bag { get; }
}