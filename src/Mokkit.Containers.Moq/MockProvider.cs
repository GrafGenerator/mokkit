using System;
using System.Collections.Generic;
using System.Linq;

namespace Mokkit.Containers.Moq;

public class MockProvider<TMock>
{
    private readonly Dictionary<Type,TMock> _mockMap;

    public MockProvider(IMockCollection<TMock> mockCollection)
    {
        _mockMap = mockCollection.ToDictionary(x => x.MockType, x => x.Mock);
    }

    public TMock? GetMock(Type mockType)
    {
        return _mockMap.TryGetValue(mockType, out var mock) ? mock : default;
    }
}