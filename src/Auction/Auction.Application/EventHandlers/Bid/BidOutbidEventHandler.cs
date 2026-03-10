using Auction.Application.Interfaces;
using Auction.Domain.Events.Bid;
using Auction.SharedKernel;
using Microsoft.Extensions.Logging;

namespace Auction.Application.EventHandlers.Bid;

public class BidOutbidEventHandler : IDomainEventHandler<BidOutbidEvent>
{
    private readonly IMessageBus _messageBus;
    private readonly ILogger<BidOutbidEventHandler> _logger;

    public BidOutbidEventHandler(
        IMessageBus messageBus,
        ILogger<BidOutbidEventHandler> logger)
    {
        _messageBus = messageBus;
        _logger = logger;
    }

    public async Task Handle(BidOutbidEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Processando evento BidOutbid: BidId={BidId}, AuctionId={AuctionId}",
                notification.BidId, notification.AuctionId);

            // Publicar evento no Kafka para notificar usuário que foi superado
            var eventData = new BidOutbidEvent(
                notification.BidId,
                notification.AuctionId,
                notification.BidderId,
                notification.PreviousAmount);

            await _messageBus.PublishAsync(
                "bid-outbid",
                eventData,
                notification.BidId.ToString(),
                cancellationToken);

            _logger.LogInformation(
                "Evento BidOutbid processado com sucesso: BidId={BidId}",
                notification.BidId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Erro ao processar evento BidOutbid: BidId={BidId}",
                notification.BidId);
        }
    }
}
