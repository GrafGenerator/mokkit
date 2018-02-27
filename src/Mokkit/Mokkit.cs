using System;
using System.Collections.Generic;

namespace Mokkit
{
    /// <summary>
    ///     Container for a set of objects (mocks) that can be instantiated
    ///     without any dependencies resolution (i.e. by default parameterless ctor).
    /// </summary>
    public class Mokkit
    {
        private readonly IMockFactory _mockFactory;
        private readonly IDictionary<Type, object> _pack = new Dictionary<Type, object>();

        public Mokkit(IMockFactory mockFactory)
        {
            _mockFactory = mockFactory;
        }

        public TMock Resolve<TMock>()
            where TMock : class
        {
            EnsureMockPresent<TMock>();
            return _pack[typeof(TMock)] as TMock;
        }

        public TMock ResolveTokenized<TMock>(ITokenizedMock<TMock> token)
            where TMock : class
        {
            EnsureMockPresent<TMock>();
            return _pack[token.GetType()] as TMock;
        }

        public Mokkit Customize<TMock>(Action<TMock> customizeFn)
            where TMock : class
        {
            EnsureMockPresent<TMock>();
            var mock = _pack[typeof(TMock)] as TMock;

            customizeFn(mock);

            return this;
        }

        private void EnsureMockPresent<TMock>()
        {
            if (!_pack.ContainsKey(typeof(TMock))) _pack.Add(typeof(TMock), _mockFactory.CreateMock<TMock>());
        }
    }
}