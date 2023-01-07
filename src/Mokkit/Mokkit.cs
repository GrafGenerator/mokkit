using System;
using System.Collections.Generic;

namespace Mokkit
{
    /// <summary>
    ///     Container for a set of objects (mocks) that can be instantiated without any direct dependencies resolution.
    ///     Any possible dependency resolution here for such kind of objects should be hidden in <see cref="IMockFactory" />.
    /// </summary>
    internal class Mokkit<TToken> : IMokkit<TToken>
    {
        private readonly IMockFactory _mockFactory;

        private readonly IDictionary<IDiscriminator<TToken>, object> _pack =
            new Dictionary<IDiscriminator<TToken>, object>();

        public Mokkit(IMockFactory mockFactory)
        {
            _mockFactory = mockFactory;
        }

        public IMokkit<TToken> Customize<TMock>(IDiscriminator<TToken> discriminator, Action<TMock> customizeFn)
            where TMock : class
        {
            // EnsureMockPresent<TMock>();
            var mock = _pack[discriminator] as TMock;

            customizeFn(mock);

            return this;
        }

        public TMocked Resolve<TMocked>(IDiscriminator<TToken> discriminator)
            where TMocked : class
        {
            EnsureMockPresent(discriminator);
            var mock = _pack[discriminator];

            if (mock == null) throw new InvalidOperationException("No mock created, fatal error.");

            var mocked = _mockFactory.ResolveMocked<TMocked>(mock);
            return mocked;
        }

        private void EnsureMockPresent(IDiscriminator<TToken> discriminator)
        {
            // if (!_pack.ContainsKey(discriminator)) _pack.Add(discriminator, _mockFactory.CreateMock<TMock>());
        }

        public TMock Resolve<TMock>() where TMock : class
        {
            throw new NotImplementedException();
        }

        public TMock ResolveTokenized<TMock>(ITokenizedMock<TMock> token) where TMock : class
        {
            throw new NotImplementedException();
        }

        public IMokkit<TToken> Customize<TMock>(Action<TMock> customizeFn) where TMock : class
        {
            throw new NotImplementedException();
        }
    }
}