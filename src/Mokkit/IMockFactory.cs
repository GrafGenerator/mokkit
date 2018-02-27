namespace Mokkit
{
    public interface IMockFactory
    {
        object CreateMock<TMock>();
    }
}