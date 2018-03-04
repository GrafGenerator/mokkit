namespace Mokkit
{
    public interface IStage<TToken>
    {
        Scenery<TToken> Scenery { get; }

        IStage<TToken> Use(IStageSetup<TToken> setup);
        IStage<TToken> Use<TStageSetup>()
            where TStageSetup : class, IStageSetup<TToken>, new();

        TActor Enter<TActor>()
            where TActor : class;
    }
}