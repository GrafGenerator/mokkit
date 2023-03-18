using System.Threading.Tasks;
using Mokkit.Playground.CaptureTests;

namespace Mokkit.Playground.SampleScenery
{
    public interface IService1
    {
        Task<int> Call(Foo foo);

        int Value { get; set; }
    }

    internal class Service1 : IService1
    {
        public Task<int> Call(Foo foo)
        {
            Value = foo.Value;
            
            return Task.FromResult(foo.Value);
        }

        public int Value { get; set; }
    }
}