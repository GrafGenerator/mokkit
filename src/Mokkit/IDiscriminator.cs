using System;

namespace Mokkit
{
    public interface IDiscriminator<out TToken>
    {
        Type Type { get; }
        TToken Token { get; }
    }
}