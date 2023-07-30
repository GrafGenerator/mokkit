namespace Mokkit.Containers;

public interface IDependencyContainer
{
    IDependencyContainerScope BeginScope();
}