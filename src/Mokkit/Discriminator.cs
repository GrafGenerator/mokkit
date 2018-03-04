using System;

namespace Mokkit
{
    public class Discriminator : IDiscriminator<string>
    {
        private static readonly string DefaultToken = null;

        private Discriminator(Type type, string token)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Token = token;
        }

        public Type Type { get; }
        public string Token { get; }

        public static IDiscriminator<string> Of<T>(string token)
        {
            if (string.Equals(token, DefaultToken)) throw new ArgumentException("Token specified cannot be used.");

            return new Discriminator(typeof(T), token);
        }

        public static IDiscriminator<string> Of<T>()
        {
            return new Discriminator(typeof(T), DefaultToken);
        }

        // ReSharper disable once MemberCanBePrivate.Global
        protected bool Equals(Discriminator other)
        {
            return Type == other.Type && string.Equals(Token, other.Token);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Discriminator) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Type.GetHashCode() * 397) ^ (Token != null ? Token.GetHashCode() : 0);
            }
        }
    }
}