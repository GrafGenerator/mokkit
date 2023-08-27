using System;

namespace Mokkit.Containers.Moq;

public class MockRegistration<TMock>
{
    public Type InnerType { get; }

    public Func<TMock> Factory { get; }

    public MockRegistration(Type innerType, Func<TMock> factory)
    {
        InnerType = innerType;
        Factory = factory;
    }
}