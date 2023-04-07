namespace Mokkit.Capture.Containers.MockContainer;

public abstract class MockContainer<TMock> : BaseDependencyContainer, IDependencyContainer
{
    private readonly MockCollection<TMock> _mockCollection;

    protected MockContainer(MockCollection<TMock> mockCollection)
    {
        _mockCollection = mockCollection;
    }

    protected abstract TMock CreateMock<T>();
    
    public IDependencyContainerScope BeginScope()
    {
        return new DependencyScope();
    }

    private class DependencyScope : IDependencyContainerScope
    {
        public DependencyScope()
        {
        }
        
        public void Dispose()
        {
        }

        public T? TryResolve<T>()
        {
            return default;
        }
    }
}