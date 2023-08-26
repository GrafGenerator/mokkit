using System.Threading.Tasks;
using Mokkit.Playground.CaptureTests;

namespace Mokkit.Playground.SampleScenery
{
    public class SampleActor
    {
        private readonly IService1 _service1;
        private readonly IService2 _service2;
        private readonly IService3 _service3;
        private readonly IService4 _service4;

        public SampleActor(IService1 service1, IService2 service2, IService3 service3, IService4 service4)
        {
            _service1 = service1;
            _service2 = service2;
            _service3 = service3;
            _service4 = service4;
        }

        public async Task<int> Act(Foo foo)
        {
            await _service1.Call(foo);

            _service3.Mocked3(foo.StringValue);
            _service4.Mocked4(foo.StringValue);

            return _service2.Call();
        }
        
        public async Task<SampleResult> ActWithResult(SampleCommand command)
        {
            _service3.Mocked3(command.Value);
            _service4.Mocked4(command.Value);
            
            return new SampleResult
            {
                Success = command.Success,
                Code = command.Code,
                Value = command.Value
            };
        }
    }
}