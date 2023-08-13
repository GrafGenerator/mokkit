using System;

namespace Mokkit.Containers.Moq;

public class MockRegistration<TMock>
{
    public Type InnerType { get; }

    public Type MockType { get; set; }

    public TMock Mock { get; }

    public MockRegistration(Type innerType, TMock mock)
    {
        InnerType = innerType;
        MockType = typeof(TMock);
        Mock = mock;
    }
}