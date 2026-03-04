using Auction.SharedKernel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Auction.Application.Services;

/// <summary>
/// Responsável por despachar Domain Events para seus handlers
/// </summary>
public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(
        IServiceProvider serviceProvider,
        ILogger<DomainEventDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var eventType = domainEvent.GetType();
        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);

        _logger.LogDebug("Dispatching domain event: {EventType}", eventType.Name);

        // Buscar todos os handlers registrados para esse tipo de evento
        var handlers = _serviceProvider.GetServices(handlerType);

        var handlersList = handlers.ToList();
        
        if (!handlersList.Any())
        {
            _logger.LogWarning("No handlers found for domain event: {EventType}", eventType.Name);
            return;
        }

        foreach (var handler in handlersList)
        {
            try
            {
                var handleMethod = handlerType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.Handle));
                if (handleMethod != null)
                {
                    var task = (Task)handleMethod.Invoke(handler, new object[] { domainEvent, cancellationToken })!;
                    await task;
                    
                    _logger.LogInformation(
                        "Domain event {EventType} handled by {HandlerType}",
                        eventType.Name,
                        handler.GetType().Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Error handling domain event {EventType} with handler {HandlerType}", 
                    eventType.Name, 
                    handler.GetType().Name);
                throw;
            }
        }
    }
}
