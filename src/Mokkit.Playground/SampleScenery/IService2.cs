namespace Mokkit.Playground.SampleScenery
{
    public interface IService2
    {
        bool IsCalled();
        
        int Call();
    }

    internal class Service2 : IService2
    {
        private bool _isCalled = false;
        public bool IsCalled()
        {
            return _isCalled;
        }

        public int Call()
        {
            _isCalled = true;
            return 42;
        }
    }
}