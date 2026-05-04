using Microsoft.AspNetCore.Mvc;
using Mokkit.Example1.Application.Features.Client.GetClient;
using Mokkit.Example1.Application.Features.Client.SaveClient;
using Mokkit.Example1.Common;

namespace Mokkit.Example1.Api.Routes.Client;

internal static class ClientController
{
    public static async Task<IResult> GetClient(
        IRequestHandler<GetClientQuery, GetClientQueryResult> handler,
        [FromRoute] Guid clientId,
        CancellationToken cancellationToken)
    {
        var query = new GetClientQuery { ClientId = clientId };
        var result = await handler.Handle(query, cancellationToken);

        if (!result.Success || result.Client == null)
        {
            return Results.NotFound(new { Message = "Client not found" });
        }

        var responseDto = new GetClientResponseDto
        {
            Id = result.Client.Id,
            Name = result.Client.Name,
            Email = result.Client.Email,
            Phone = result.Client.Phone,
            Status = result.Client.Status,
            CreatedAt = result.Client.CreatedAt,
            UpdatedAt = result.Client.UpdatedAt
        };
        
        return Results.Ok(responseDto);
    }

    public static async Task<IResult> CreateClient(
        IRequestHandler<SaveClientCommand, SaveClientCommandResult> handler,
        [FromBody] SaveClientRequestDto dto,
        CancellationToken cancellationToken)
    {
        var command = MapDtoToSaveClientCommand(dto, SaveOperationKind.Create);
        var result = await handler.Handle(command, cancellationToken);

        var responseDto = new SaveClientResponseDto
        {
            Success = result.Success,
            ClientId = result.ClientId ?? Guid.Empty,
            Message = result.Exception?.Message
        };

        if (result.Success)
        {
            return Results.Created($"/api/v1/clients/{result.ClientId}", responseDto);
        }
        
        return Results.BadRequest(responseDto);
    }

    public static async Task<IResult> UpdateClient(
        IRequestHandler<SaveClientCommand, SaveClientCommandResult> handler,
        [FromRoute] Guid clientId,
        [FromBody] SaveClientRequestDto dto,
        CancellationToken cancellationToken)
    {
        var command = MapDtoToSaveClientCommand(dto, SaveOperationKind.Update, clientId);
        var result = await handler.Handle(command, cancellationToken);

        var responseDto = new SaveClientResponseDto
        {
            Success = result.Success,
            ClientId = result.ClientId ?? clientId,
            Message = result.Exception?.Message
        };

        if (result.Success)
        {
            return Results.Ok(responseDto);
        }
        
        return Results.BadRequest(responseDto);
    }
    
    private static SaveClientCommand MapDtoToSaveClientCommand(SaveClientRequestDto dto, SaveOperationKind operation, Guid? id = null)
    {
        return new SaveClientCommand
        {
            Operation = operation,
            ClientData = new SaveClientData
            {
                Id = id,
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                Status = dto.Status
            }
        };
    }
}
