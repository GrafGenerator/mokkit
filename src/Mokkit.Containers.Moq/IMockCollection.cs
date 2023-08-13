using System.Collections.Generic;

namespace Mokkit.Containers.Moq;

public interface IMockCollection<TMock> : IList<MockRegistration<TMock>>
{
    IMockCollection<TMock> AddMock<T>(TMock mock);

    IMockCollection<TMock> TryAddMock<T>(TMock mock);
    
    IReadOnlyCollection<MockRegistration<TMock>> Registrations { get; }
}