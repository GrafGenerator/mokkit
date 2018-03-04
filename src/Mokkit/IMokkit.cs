using System;

namespace Mokkit
{
    public interface IMokkit<TToken>
    {
        TMock Resolve<TMock>()
            where TMock : class;

        TMock ResolveTokenized<TMock>(ITokenizedMock<TMock> token)
            where TMock : class;

        IMokkit<TToken> Customize<TMock>(Action<TMock> customizeFn)
            where TMock : class;
    }
}