using Auction.Application.Interfaces;
using Auction.Application.Interfaces.Repositories;
using Auction.Domain.Events.Bid;
using Auction.Domain.ValueObjects;
using Auction.Infrastructure.Options;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Auction.Infrastructure.Consumers;

/// <summary>
/// Consumer Kafka que processa lances de forma assíncrona
/// Reduz concorrência no endpoint HTTP fazendo processamento em background
/// </summary>
public class BidPlacementRequestedConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BidPlacementRequestedConsumer> _logger;
    private readonly KafkaOptions _kafkaOptions;
    private readonly IConsumer<string, string> _consumer;

    public BidPlacementRequestedConsumer(
        IServiceProvider serviceProvider,
        IOptions<KafkaOptions> kafkaOptions,
        ILogger<BidPlacementRequestedConsumer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _kafkaOptions = kafkaOptions.Value;

        var config = new ConsumerConfig
        {
            BootstrapServers = _kafkaOptions.BootstrapServers,
            GroupId = "bid-placement-consumer-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            MaxPollIntervalMs = 300000, // 5 minutos
            SessionTimeoutMs = 45000,
            HeartbeatIntervalMs = 3000
        };

        _consumer = new ConsumerBuilder<string, string>(config)
            .SetErrorHandler((_, e) => _logger.LogError("Kafka error: {Reason}", e.Reason))
            .Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe("bid-placement-requested");

        _logger.LogInformation("BidPlacementRequestedConsumer iniciado. Aguardando mensagens...");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(stoppingToken);

                    if (consumeResult?.Message?.Value != null)
                    {
                        await ProcessBidPlacementAsync(consumeResult.Message.Value, stoppingToken);
                        _consumer.Commit(consumeResult);
                    }
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Erro ao consumir mensagem do Kafka");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar lance");
                }
            }
        }
        finally
        {
            _consumer.Close();
            _consumer.Dispose();
        }
    }

    private async Task ProcessBidPlacementAsync(string message, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var auctionRepository = scope.ServiceProvider.GetRequiredService<IAuctionRepository>();
        var bidRepository = scope.ServiceProvider.GetRequiredService<IBidRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();
        var messageBus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        BidPlacementRequest? request = null;

        try
        {
            // 1. Deserializar mensagem
            request = JsonSerializer.Deserialize<BidPlacementRequest>(message);
            if (request is null)
            {
                _logger.LogWarning("Mensagem inválida recebida: {Message}", message);
                return;
            }

            _logger.LogInformation(
                "Processando lance assíncrono: BidId={BidId}, AuctionId={AuctionId}, Amount={Amount}",
                request.BidId, request.AuctionId, request.Amount);

            // 2. Verificar idempotência (se lance já foi processado)
            var existingBid = await bidRepository.GetByIdAsync(request.BidId, cancellationToken);
            if (existingBid is not null)
            {
                _logger.LogInformation(
                    "Lance já processado anteriormente (idempotência): BidId={BidId}",
                    request.BidId);
                return;
            }

            // 3. Buscar leilão com controle de concorrência
            var auction = await auctionRepository.GetByIdAsync(request.AuctionId, cancellationToken);
            if (auction is null)
            {
                _logger.LogWarning(
                    "Leilão não encontrado ao processar lance: AuctionId={AuctionId}",
                    request.AuctionId);
                return;
            }

            // 4. Criar Value Object Money
            var amountResult = Money.Create(request.Amount, request.Currency);
            if (!amountResult.IsSuccess)
            {
                _logger.LogWarning(
                    "Valor inválido ao processar lance: BidId={BidId}, Amount={Amount}",
                    request.BidId, request.Amount);
                return;
            }

            // 5. Criar entidade Bid
            var bidResult = Domain.Entities.Bid.Create(
                request.AuctionId,
                request.BidderId,
                amountResult.Value,
                request.IsAutoBid,
                request.BidId); // Usar BidId gerado no handler

            if (!bidResult.IsSuccess)
            {
                _logger.LogWarning(
                    "Erro ao criar entidade Bid: BidId={BidId}, Error={Error}",
                    request.BidId, bidResult.Error.Message);
                return;
            }

            var bid = bidResult.Value;

            // 6. Marcar lance anterior como "outbid"
            if (auction.CurrentWinningBidId.HasValue)
            {
                var previousBid = await bidRepository.GetByIdAsync(
                    auction.CurrentWinningBidId.Value,
                    cancellationToken);

                if (previousBid is not null)
                {
                    previousBid.Outbid();
                    bidRepository.Update(previousBid);
                }
            }

            // 7. Registrar lance no leilão
            var registerResult = auction.RegisterBid(bid.Id, amountResult.Value, request.BidderId);
            if (!registerResult.IsSuccess)
            {
                _logger.LogWarning(
                    "Erro ao registrar lance no leilão: BidId={BidId}, Error={Error}",
                    request.BidId, registerResult.Error.Message);
                return;
            }

            // 8. Persistir mudanças com controle de concorrência
            await bidRepository.AddAsync(bid, cancellationToken);
            auctionRepository.Update(auction);

            var maxRetries = 3;
            var retryCount = 0;
            var saved = false;

            while (!saved && retryCount < maxRetries)
            {
                try
                {
                    await unitOfWork.SaveChangesAsync(cancellationToken);
                    saved = true;
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("modificado por outro processo"))
                {
                    retryCount++;

                    _logger.LogWarning(
                        "Conflito de concorrência (tentativa {RetryCount}/{MaxRetries}): BidId={BidId}, AuctionId={AuctionId}",
                        retryCount, maxRetries, request.BidId, request.AuctionId);

                    if (retryCount >= maxRetries)
                    {
                        _logger.LogError(
                            "Falha ao processar lance após {MaxRetries} tentativas: BidId={BidId}",
                            maxRetries, request.BidId);
                        return;
                    }

                    // Aguardar antes de retentar (backoff exponencial)
                    await Task.Delay(TimeSpan.FromMilliseconds(100 * Math.Pow(2, retryCount)), cancellationToken);

                    // Recarregar entidades do banco
                    auction = await auctionRepository.GetByIdAsync(request.AuctionId, cancellationToken);
                    if (auction is null)
                    {
                        _logger.LogError("Leilão não encontrado na retentativa: AuctionId={AuctionId}", request.AuctionId);
                        return;
                    }

                    // Tentar registrar novamente
                    registerResult = auction.RegisterBid(bid.Id, amountResult.Value, request.BidderId);
                    if (!registerResult.IsSuccess)
                    {
                        _logger.LogWarning(
                            "Erro ao registrar lance na retentativa: BidId={BidId}, Error={Error}",
                            request.BidId, registerResult.Error.Message);
                        return;
                    }

                    auctionRepository.Update(auction);
                }
            }

            // 9. Invalidar cache do leilão
            await cacheService.RemoveAsync($"auction:{request.AuctionId}");
            await cacheService.RemoveAsync($"auction:{request.AuctionId}:bids");
            await cacheService.RemoveAsync($"auction:{request.AuctionId}:highest-bid");

            _logger.LogInformation(
                "Cache invalidado para leilão: AuctionId={AuctionId}",
                request.AuctionId);

            // 10. Publicar evento de sucesso (BidPlacedEvent)
            var bidPlacedEvent = new BidPlacedEvent(
                bid.Id,
                bid.AuctionId,
                bid.BidderId,
                bid.Amount.Value,
                bid.IsAutoBid,
                bid.BidTime);

            await messageBus.PublishAsync(
                "bid-placed",
                bidPlacedEvent,
                bid.Id.ToString(),
                cancellationToken);

            _logger.LogInformation(
                "Lance processado com sucesso: BidId={BidId}, AuctionId={AuctionId}, Amount={Amount}",
                request.BidId, request.AuctionId, request.Amount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Erro crítico ao processar lance: BidId={BidId}, AuctionId={AuctionId}",
                request?.BidId, request?.AuctionId);
        }
    }

    public override void Dispose()
    {
        _consumer?.Close();
        _consumer?.Dispose();
        base.Dispose();
    }
}

/// <summary>
/// DTO para deserializar mensagem do Kafka (bid-placement-requested)
/// </summary>
public record BidPlacementRequest(
    Guid BidId,
    Guid AuctionId,
    Guid BidderId,
    decimal Amount,
    string Currency,
    bool IsAutoBid,
    string IdempotencyKey,
    DateTime RequestedAt);
