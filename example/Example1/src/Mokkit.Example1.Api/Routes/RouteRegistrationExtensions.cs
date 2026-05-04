using System.Net.Mime;
using Mokkit.Example1.Api.Routes.Client;

namespace Mokkit.Example1.Api.Routes;

public static class RouteRegistrationExtensions
{
    public static IEndpointRouteBuilder MapApiRoutes(this IEndpointRouteBuilder parentGroup)
    {
        return parentGroup
            .MapClientApi();
    }

    private static IEndpointRouteBuilder MapClientApi(this IEndpointRouteBuilder routeBuilder)
    {
        var groupBuilder = routeBuilder
            .MapGroup("api")
            .MapGroup("v1")
            .MapGroup("clients")
            .WithTags("Client");

        groupBuilder.MapGet("{clientId:guid}", ClientController.GetClient)
            .Produces<GetClientResponseDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("GetClient");

        groupBuilder.MapPost("", ClientController.CreateClient)
            .Produces<SaveClientResponseDto>(StatusCodes.Status201Created, MediaTypeNames.Application.Json)
            .Produces<SaveClientResponseDto>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)
            .WithName("CreateClient");

        groupBuilder.MapPut("{clientId:guid}", ClientController.UpdateClient)
            .Produces<SaveClientResponseDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .Produces<SaveClientResponseDto>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)
            .WithName("UpdateClient");
        
        return routeBuilder;
    }
}