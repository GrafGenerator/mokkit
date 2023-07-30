namespace Mokkit.Capture.Containers;

public interface IDependencyContainer
{
    IDependencyContainerScope BeginScope();
}