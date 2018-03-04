namespace Mokkit
{
    public abstract class StageBase<TToken> : IStage<TToken>
    {
        protected StageBase(Scenery<TToken> scenery)
        {
            Scenery = scenery;
        }

        public Scenery<TToken> Scenery { get; }

        public IStage<TToken> Use(IStageSetup<TToken> setup)
        {
            setup.SetupMocks(Scenery.Mokkit);
            return this;
        }

        public IStage<TToken> Use<TStageSetup>()
            where TStageSetup : class, IStageSetup<TToken>, new()
        {
            return Use(new TStageSetup());
        }

        public TActor Enter<TActor>()
            where TActor : class
        {
            // todo: resolve actor here from all supplies
            return null;
        }
    }
}