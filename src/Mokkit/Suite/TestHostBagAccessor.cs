using System.Threading;

namespace Mokkit.Suite;

/// <summary>
/// Internal implementation of <see cref="ITestHostBagAccessor"/> that provides thread-safe access to the test host bag.
/// This class uses AsyncLocal storage to maintain test host bag context across async operations.
/// </summary>
internal class TestHostBagAccessor : ITestHostBagAccessor
{
    private static readonly AsyncLocal<TestHostBagHolder> TestHostBagCurrent = new();
    
    /// <summary>
    /// Gets or sets the test host bag associated with the current thread.
    /// </summary>
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

