using System.Threading.Tasks;
using Mokkit.Playground.CaptureTests;

namespace Mokkit.Playground.SampleScenery
{
    public class SampleActor
    {
        private readonly IService1 _service1;
        private readonly IService2 _service2;
        private readonly IService3 _service3;

        public SampleActor(IService1 service1, IService2 service2, IService3 service3)
        {
            _service1 = service1;
            _service2 = service2;
            _service3 = service3;
        }

        public async Task<int> Act(Foo foo)
        {
            await _service1.Call(foo);

            _service3.Mocked(foo.Value.ToString());

            return _service2.Call();
        }
    }
}