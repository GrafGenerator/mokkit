using System;

namespace Mokkit.Suite;

public class TestHostContext
{
    public TestHostContext(TestHostBagResolver testHostBagResolver, Guid testHostId)
    {
        TestHostBagResolver = testHostBagResolver;
        TestHostId = testHostId;
    }

    public TestHostBagResolver TestHostBagResolver { get; }
    
    public Guid TestHostId { get; }
}