using Auction.Application.Interfaces;
using Auction.Domain.Events.Auction;
using Auction.SharedKernel;
using Microsoft.Extensions.Logging;

namespace Auction.Application.EventHandlers;

/// <summary>
/// Handler que escuta Domain Events e publica Integration Events no Kafka
/// </summary>
public class AuctionCancelledEventHandler : IDomainEventHandler<AuctionCancelledEvent>
{
    private readonly IMessageBus _messageBus;
    private readonly ILogger<AuctionCancelledEventHandler> _logger;

    public AuctionCancelledEventHandler(
        IMessageBus messageBus,
        ILogger<AuctionCancelledEventHandler> logger)
    {
        _messageBus = messageBus;
        _logger = logger;
    }

    public async Task Handle(AuctionCancelledEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling AuctionCancelledEvent for Auction: {AuctionId}, Reason: {Reason}",
            domainEvent.AuctionId,
            domainEvent.Reason);

        try
        {
            // Publicar Integration Event no Kafka para outros serviços/microserviços
            await _messageBus.PublishAsync(
                topic: "auction.cancelled",
                @event: domainEvent,
                partitionKey: domainEvent.AuctionId.ToString(),
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Integration event published to Kafka topic 'auction.cancelled' for Auction: {AuctionId}",
                domainEvent.AuctionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error publishing integration event to Kafka for Auction: {AuctionId}",
                domainEvent.AuctionId);

            // Re-throw para garantir que a transação seja revertida
            throw;
        }
    }
}
