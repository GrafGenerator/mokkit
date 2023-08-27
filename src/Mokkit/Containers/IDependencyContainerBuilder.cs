using System.Collections.Generic;
using System.Threading.Tasks;
using Mokkit.Suite;

namespace Mokkit.Containers;

public interface IDependencyContainerBuilder
{
    Task PreInit();
    
    Task Init();
    
    Task PreBuild(IDependencyContainerBuilder[] builders);

    TCollection? TryGetCollection<TCollection>() where TCollection : class;
    
    IDependencyContainer Build();
}