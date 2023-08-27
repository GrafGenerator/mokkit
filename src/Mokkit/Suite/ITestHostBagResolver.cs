using System;

namespace Mokkit.Suite;

public interface ITestHostBagResolver
{
    TestHostBag Get(Guid testHostId);
}