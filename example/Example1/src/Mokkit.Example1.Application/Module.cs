using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Mokkit.Example1.Application.Features.Client.GetClient;
using Mokkit.Example1.Application.Features.Client.SaveClient;
using Mokkit.Example1.Application.Logic.Persistence;
using Mokkit.Example1.Common;

namespace Mokkit.Example1.Application;

public static class Module
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddSingleton<IIdGenerator, GuidIdGenerator>();

        services.AddValidatorsFromAssemblyContaining<SaveClientCommandValidator>(
            ServiceLifetime.Singleton,
            includeInternalTypes: true);

        services.AddScoped(
            typeof(IRequestHandler<SaveClientCommand, SaveClientCommandResult>),
            typeof(SaveClientCommandHandler));
            
        services.AddScoped(
            typeof(IRequestHandler<GetClientQuery, GetClientQueryResult>),
            typeof(GetClientQueryHandler));

        return services;
    }
}