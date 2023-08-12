using System.Collections.Generic;

namespace Mokkit.Containers.MockContainer;

public interface IMockCollection<TMock>: IList<MockRegistration<TMock>>
{
    IMockCollection<TMock> AddMock<T>(TMock mock);

    IMockCollection<TMock> TryAddMock<T>(TMock mock);

    TMock? GetMock<T>();

    TMock GetRequiredMock<T>();
    
    IReadOnlyCollection<MockRegistration<TMock>> Registrations { get; }
}
