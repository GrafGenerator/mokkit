using System.Threading.Tasks;
using Mokkit.Playground.CaptureTests;

namespace Mokkit.Playground.SampleScenery
{
    public interface IService1
    {
        Task<int> Call(Foo foo);
    }

    internal class Service1 : IService1
    {
        public Task<int> Call(Foo foo)
        {
            return Task.FromResult(foo.Value);
        }
    }
}