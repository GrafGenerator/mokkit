using System;
using System.Collections.Generic;

namespace Mokkit.Containers.Moq;

public interface IMockCollection<TMock> : IList<MockRegistration<TMock>>
{
    IMockCollection<TMock> AddMock<T>(Func<TMock> factory);

    IMockCollection<TMock> TryAddMock<T>(Func<TMock> factory);
    
    IReadOnlyCollection<MockRegistration<TMock>> Registrations { get; }
}