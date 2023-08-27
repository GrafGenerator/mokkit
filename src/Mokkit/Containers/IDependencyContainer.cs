using Mokkit.Suite;

namespace Mokkit.Containers;

public interface IDependencyContainer
{
    IDependencyContainerScope BeginScope(TestHostContext context);
}