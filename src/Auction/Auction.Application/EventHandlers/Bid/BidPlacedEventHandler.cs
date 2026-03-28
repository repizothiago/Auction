using Auction.Application.Interfaces;
using Auction.Domain.Events.Bid;
using Auction.SharedKernel;
using Microsoft.Extensions.Logging;

namespace Auction.Application.EventHandlers.Bid;

/// <summary>
/// Handler para BidPlacedEvent - Publica notificação para outros sistemas
/// Cache já é invalidado pelo BidPlacementRequestedConsumer
/// </summary>
public class BidPlacedEventHandler : IDomainEventHandler<BidPlacedEvent>
{
    private readonly IMessageBus _messageBus;
    private readonly ILogger<BidPlacedEventHandler> _logger;

    public BidPlacedEventHandler(
        IMessageBus messageBus,
        ILogger<BidPlacedEventHandler> logger)
    {
        _messageBus = messageBus;
        _logger = logger;
    }

    public async Task Handle(BidPlacedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "[EventoDominio] Processando BidPlacedEvent: LanceId={LanceId}, AuctionId={AuctionId}, Valor={Valor}",
                notification.BidId, notification.AuctionId, notification.Amount);

            // Publicar evento no Kafka para notificações em tempo real (SignalR, email, push)
            var eventData = new BidPlacedEvent(
                notification.BidId,
                notification.AuctionId,
                notification.BidderId,
                notification.Amount,
                notification.IsAutoBid,
                notification.BidTime);

            await _messageBus.PublishAsync(
                "bid-placed-notification",
                eventData,
                notification.BidId.ToString(),
                cancellationToken);

            _logger.LogInformation(
                "[Mensageria] Evento BidPlaced publicado para notificações: LanceId={LanceId}",
                notification.BidId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[EventoDominio] Erro ao processar BidPlacedEvent: LanceId={LanceId}",
                notification.BidId);

            // Não propagar exceção para não falhar a transação principal
        }
    }
}
