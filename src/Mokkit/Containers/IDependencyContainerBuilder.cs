using System.Threading.Tasks;

namespace Mokkit.Capture.Containers;

public interface IDependencyContainerBuilder
{
    Task PreInit();
    
    Task Init();
    
    Task PreBuild();
    
    IDependencyContainer Build();
}