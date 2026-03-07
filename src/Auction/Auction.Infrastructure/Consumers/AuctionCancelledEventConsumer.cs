using Auction.Application.Interfaces.Repositories;
using Auction.Domain.Events.Auction;
using Auction.Infrastructure.Messaging;
using Auction.Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Auction.Infrastructure.Consumers;

/// <summary>
/// Consumer que processa eventos de cancelamento de leilão do Kafka
/// </summary>
public class AuctionCancelledEventConsumer : KafkaConsumerBase<AuctionCancelledEvent>
{
    public AuctionCancelledEventConsumer(IOptions<KafkaOptions> kafkaOptions, IServiceProvider serviceProvider, ILogger<AuctionCancelledEventConsumer> logger)
        : base(kafkaOptions, serviceProvider, logger, topic: kafkaOptions.Value.Topics.AuctionCancelled, consumerGroupId: kafkaOptions.Value.ConsumerGroups.AuctionService)
    {
    }

    protected override async Task ProcessEventAsync(
        AuctionCancelledEvent @event,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        Logger.LogInformation(
            "Processing AuctionCancelledEvent - AuctionId: {AuctionId}, Reason: {Reason}, EventId: {EventId}, OccurredOn: {OccurredOn}",
            @event.AuctionId,
            @event.Reason,
            @event.EventId,
            @event.OccurredOn);

        // Buscar o leilão e executar ações necessárias
        using var scope = serviceProvider.CreateScope();
        var auctionRepository = scope.ServiceProvider.GetRequiredService<IAuctionRepository>();

        var auction = await auctionRepository.GetByIdAsync(@event.AuctionId, cancellationToken);

        if (auction is not null)
        {
            Logger.LogInformation(
                "Auction '{Title}' was cancelled. Status: {Status}",
                auction.Title,
                auction.Status);

            Console.WriteLine("Test");

            // Aqui você pode:
            // - Enviar notificações para participantes do leilão
            // - Invalidar caches relacionados ao leilão
            // - Atualizar sistemas de analytics
            // - Integrar com serviços de notificação (email, push, SMS)
            // - Registrar em sistema de auditoria
            // - Processar reembolsos automáticos se necessário

            // Exemplo: Notificar participantes (implementação futura)
            // var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            // await notificationService.NotifyAuctionCancelled(auction, @event.Reason, cancellationToken);
        }
        else
        {
            Logger.LogWarning(
                "Auction {AuctionId} not found when processing cancellation event",
                @event.AuctionId);
        }

        await Task.CompletedTask;
    }
}

