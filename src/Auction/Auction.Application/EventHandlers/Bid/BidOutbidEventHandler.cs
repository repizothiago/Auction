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
                "[EventoDominio] Processando BidOutbidEvent: LanceId={LanceId}, AuctionId={AuctionId}",
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
                "[Mensageria] Evento BidOutbid publicado com sucesso: LanceId={LanceId}",
                notification.BidId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[EventoDominio] Erro ao processar BidOutbidEvent: LanceId={LanceId}",
                notification.BidId);
        }
    }
}
