namespace Mokkit
{
    public class Scenery<TMokkitToken>
    {
        private readonly IMockFactory _mockFactory;

        public Scenery(IMockFactory mockFactory)
        {
            _mockFactory = mockFactory;

            Mokkit = new Mokkit<TMokkitToken>(mockFactory);
        }

        public IMokkit<TMokkitToken> Mokkit { get; }
    }
}