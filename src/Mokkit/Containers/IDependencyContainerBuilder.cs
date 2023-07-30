using System.Threading.Tasks;

namespace Mokkit.Containers;

public interface IDependencyContainerBuilder
{
    Task PreInit();
    
    Task Init();
    
    Task PreBuild();
    
    IDependencyContainer Build();
}