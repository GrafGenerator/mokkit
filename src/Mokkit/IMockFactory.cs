namespace Mokkit
{
    public interface IMockFactory
    {
        TMock CreateMock<TMock>();

        TMocked ResolveMocked<TMocked>(object mock);
    }
}