using System;

namespace Mokkit.Containers.MockContainer;

public class MockRegistration<TMock>
{
    public Type Type { get; }

    public TMock Mock { get; }

    public MockRegistration(Type type, TMock mock)
    {
        Type = type;
        Mock = mock;
    }
}