namespace Mokkit.Playground.SampleScenery
{
    public class SampleActor
    {
        private readonly IService1 _service1;
        private readonly IService2 _service2;

        public SampleActor(IService1 service1, IService2 service2)
        {
            _service1 = service1;
            _service2 = service2;
        }

        public void Act()
        {
            _service1.Call1();
            _service2.Call2();
        }
    }
}