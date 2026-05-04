using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mokkit.Example1.Application.Logic.Messages;
using Mokkit.Example1.Application.Logic.Persistence;
using Mokkit.Example1.Common;
using Mokkit.Example1.Db.Postgres;
using Mokkit.Example1.Domain.Entities;

namespace Mokkit.Example1.Application.Features.Client.SaveClient;

/// <summary>
/// Handler for command <see cref="SaveClientCommand"/>
/// </summary>
internal sealed class SaveClientCommandHandler : IRequestHandler<SaveClientCommand, SaveClientCommandResult>
{
    private readonly IValidator<SaveClientCommand> _validator;
    private readonly ExampleContext _dbContext;
    private readonly IClientCacheService _cacheService;
    private readonly IKafkaEventPublisher _eventPublisher;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IIdGenerator _idGenerator;
    private readonly ILogger<SaveClientCommandHandler> _logger;

    /// <summary>
    /// Handler for command <see cref="SaveClientCommand"/>
    /// </summary>
    public SaveClientCommandHandler(
        IValidator<SaveClientCommand> validator,
        ExampleContext dbContext,
        IClientCacheService cacheService,
        IKafkaEventPublisher eventPublisher,
        IDateTimeProvider dateTimeProvider,
        IIdGenerator idGenerator,
        ILogger<SaveClientCommandHandler> logger)
    {
        _validator = validator;
        _dbContext = dbContext;
        _cacheService = cacheService;
        _eventPublisher = eventPublisher;
        _dateTimeProvider = dateTimeProvider;
        _idGenerator = idGenerator;
        _logger = logger;
    }

    public async ValueTask<SaveClientCommandResult> Handle(
        SaveClientCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var data = command.ClientData;
            
            _logger.LogInformation("Starting {Operation} client operation for ID: {ClientId}", 
                command.Operation, data.Id);

            var validationResult = await _validator.ValidateAsync(command, cancellationToken);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation failed for {Operation} client operation: {Errors}",
                    command.Operation, string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                
                return new SaveClientCommandResult(false, data.Id,
                    new ValidationException("Validation failed", validationResult.Errors));
            }

            Domain.Entities.Client client;
            
            if (command.Operation == SaveOperationKind.Update && data.Id.HasValue)
            {
                client = await _dbContext.Clients
                    .FirstOrDefaultAsync(c => c.Id == data.Id.Value, cancellationToken);
                
                if (client == null)
                {
                    _logger.LogWarning("Client not found for update operation: {ClientId}", data.Id);
                    return new SaveClientCommandResult(false, data.Id, 
                        new InvalidOperationException("Client not found"));
                }
                
                // Update existing client
                client.Name = data.Name;
                client.Email = data.Email;
                client.Phone = data.Phone;
                client.Status = (ClientStatus)data.Status;
                client.UpdatedAt = _dateTimeProvider.UtcNow;
                
                _logger.LogInformation("Updated client: {ClientId}", client.Id);
            }
            else
            {
                // Create new client
                var now = _dateTimeProvider.UtcNow;
                client = new Domain.Entities.Client
                {
                    Id = _idGenerator.NewId(),
                    Name = data.Name,
                    Email = data.Email,
                    Phone = data.Phone,
                    Status = (ClientStatus)data.Status,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                
                _dbContext.Clients.Add(client);
                _logger.LogInformation("Created new client: {ClientId}", client.Id);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            
            // Update cache
            await _cacheService.SetClientAsync(client, cancellationToken);
            
            // Publish event
            var eventType = command.Operation == SaveOperationKind.Create ? "created" : "updated";
            await _eventPublisher.PublishClientEventAsync(client.Id, eventType, cancellationToken);
            
            _logger.LogInformation("Successfully {Operation} client: {ClientId}", 
                command.Operation.ToString().ToLowerInvariant(), client.Id);

            return new SaveClientCommandResult(true, client.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during {Operation} client operation for ID: {ClientId}", 
                command.Operation, command.ClientData.Id);

            return new SaveClientCommandResult(false, command.ClientData.Id, ex);
        }
    }
}
