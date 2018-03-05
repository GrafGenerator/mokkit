using System;
using Mokkit.Playground.SampleScenery;
using Mokkit.Playground.Setups;

namespace Mokkit.Playground
{
    public class SimplePlayground
    {
        private IStage<string> _stage;

        public void Setup()
        {
            _stage = NewStage()
                .Use<CoreSetup>();
        }

        public void Test()
        {
            var actor = _stage.Enter<SampleActor>();
        }

        private static IStage<string> NewStage()
        {
            var scenery = new Scenery<string>(new TestMockFactory());
            return new Stage(scenery);
        }

        private class TestMockFactory : IMockFactory
        {
            public TMock CreateMock<TMock>()
            {
                throw new NotImplementedException();
            }

            public TMocked ResolveMocked<TMocked>(object mock)
            {
                throw new NotImplementedException();
            }
        }
    }
}