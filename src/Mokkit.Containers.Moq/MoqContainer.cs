using Moq;

namespace Mokkit.Containers.Moq;

public class MoqContainer : BaseDependencyContainer, IDependencyContainer
{
    private readonly MockProvider<Mock> _mockProvider;

    internal MoqContainer(MockProvider<Mock> mockProvider)
    {
        _mockProvider = mockProvider;
    }

    public IDependencyContainerScope BeginScope()
    {
        return new MockScope(_mockProvider);
    }

    private class MockScope : IDependencyContainerScope
    {
        private readonly MockProvider<Mock> _mockProvider;

        public MockScope(MockProvider<Mock> mockProvider)
        {
            _mockProvider = mockProvider;
        }
        
        public void Dispose()
        {
        }

        public T? TryResolve<T>() where T : class
        {
            return _mockProvider.GetMock(typeof(T)) as T;
        }
    }
}