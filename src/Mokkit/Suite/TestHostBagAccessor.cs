using System.Threading;

namespace Mokkit.Suite;

internal class TestHostBagAccessor : ITestHostBagAccessor
{
    private static readonly AsyncLocal<TestHostBagHolder> TestHostBagCurrent = new();
    
    public TestHostBag? Bag
    {
        get => TestHostBagCurrent.Value?.Bag;
        set
        {
            if (TestHostBagCurrent.Value != null)
            {
                TestHostBagCurrent.Value.Bag = null;
            }

            if (value != null)
            {
                TestHostBagCurrent.Value = new TestHostBagHolder { Bag = value };
            }
        }
    }
    
    private class TestHostBagHolder
    {
        public TestHostBag Bag;
    }
}

