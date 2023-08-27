using System;

namespace Mokkit.Containers.Microsoft.Extensions.DependencyInjection;

public interface IStageResolve
{
    object? Resolve(Type serviceType);
}