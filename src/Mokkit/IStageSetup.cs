namespace Mokkit
{
    public interface IStageSetup<TMokkitToken>
    {
        void SetupMocks(IMokkit<TMokkitToken> mokkit);
    }
}