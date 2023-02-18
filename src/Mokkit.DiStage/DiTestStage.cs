using Microsoft.Extensions.DependencyInjection;
using Mokkit.Capture.Suite;

namespace Mokkit.DiStage;

public class DiTestStage : TestStage
{
    private IServiceScopeFactory _scopeFactory;

    public DiTestStage()
    {
        
    }
}