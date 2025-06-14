using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mokkit.Playground.CaptureTests;

namespace Mokkit.Playground.SampleScenery
{
    public class SampleScopedActor
    {
        private readonly IServiceProvider _serviceProvider;

        public SampleScopedActor(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<string> Act(Foo foo)
        {
            using var scope = _serviceProvider.CreateScope();

            var service3 = scope.ServiceProvider.GetRequiredService<IService3>();
            return service3.Mocked3($"scope: {foo.StringValue}");
        }
    }
}