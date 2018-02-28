namespace Mokkit
{
    public static class Scenery
    {
        public static IMokkit Mokkit(IMockFactory factory)
        {
            return new Mokkit(factory);
        }
    }
}