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
        MockType = mock?.GetType() ?? throw new ArgumentNullException(nameof(mock));
        Mock = mock;
    }
}