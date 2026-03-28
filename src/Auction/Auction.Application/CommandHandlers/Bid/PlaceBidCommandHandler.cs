using Auction.Application.Commands.Bid;
using Auction.Application.Interfaces;
using Auction.Application.Interfaces.Repositories;
using Auction.Domain.Events.Bid;
using Auction.Domain.ValueObjects;
using Auction.SharedKernel;
using Auction.SharedKernel.Errors;
using Microsoft.Extensions.Logging;

namespace Auction.Application.CommandHandlers.Bid;

/// <summary>
/// Handler para PlaceBidCommand - APENAS VALIDAÇÕES
/// O processamento real é feito de forma assíncrona pelo BidPlacedEventConsumer
/// </summary>
public class PlaceBidCommandHandler
{
    private readonly IAuctionRepository _auctionRepository;
    private readonly IBidRepository _bidRepository;
    private readonly IMessageBus _messageBus;
    private readonly ILogger<PlaceBidCommandHandler> _logger;

    public PlaceBidCommandHandler(
        IAuctionRepository auctionRepository,
        IBidRepository bidRepository,
        IMessageBus messageBus,
        ILogger<PlaceBidCommandHandler> logger)
    {
        _auctionRepository = auctionRepository;
        _bidRepository = bidRepository;
        _messageBus = messageBus;
        _logger = logger;
    }

    public async Task<Result<Guid>> HandleAsync(PlaceBidCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "[Comando] Validando lance: AuctionId={AuctionId}, LicitanteId={LicitanteId}, Valor={Valor}",
                request.AuctionId, request.BidderId, request.Bid.Value);

            // 1. Buscar leilão (apenas para validações rápidas)
            var auction = await _auctionRepository.GetByIdAsync(request.AuctionId, cancellationToken);
            if (auction is null)
                return Result<Guid>.Failure(Error.NotFound("Bid.AuctionNotFound", "Leilão não encontrado"));

            // 2. Validações básicas (rápidas)
            var validationResult = await ValidateBidRules(auction, request.BidderId, request.Bid.Value, cancellationToken);
            if (!validationResult.IsSuccess)
                return Result<Guid>.Failure(validationResult.Error);

            // 3. Gerar BidId único
            var bidId = Guid.NewGuid();

            // 4. Publicar evento no Kafka para processamento assíncrono
            var bidPlacementEvent = new BidPlacementRequestedEvent(
                bidId,
                request.AuctionId,
                request.BidderId,
                request.Bid.Value,
                request.Bid.Currency,
                false,
                request.IdempotencyKey,
                DateTime.UtcNow);

            await _messageBus.PublishAsync(
                "bid-placement-requested",
                bidPlacementEvent,
                bidId.ToString(),
                cancellationToken);

            _logger.LogInformation(
                "[Comando] Lance enviado para processamento assíncrono: LanceId={LanceId}, AuctionId={AuctionId}",
                bidId, request.AuctionId);

            // Retornar BidId imediatamente (processamento será feito pelo consumer)
            return Result<Guid>.Success(bidId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[Comando] Erro ao validar lance: AuctionId={AuctionId}, LicitanteId={LicitanteId}",
                request.AuctionId, request.BidderId);

            return Result<Guid>.Failure(
                Error.Failure("Bid.ValidationError", "Erro ao validar lance. Tente novamente."));
        }
    }

    private async Task<Result> ValidateBidRules(
        Domain.Entities.Auction auction,
        Guid bidderId,
        decimal amount,
        CancellationToken cancellationToken)
    {
        // 1. Validar status do leilão
        if (auction.Status != Domain.Enum.AuctionStatus.Active)
            return Result.Failure(
                Error.Failure("Bid.AuctionNotActive", "Leilão não está ativo"));

        // 2. Validar se vendedor está tentando dar lance
        if (bidderId == auction.SellerId)
            return Result.Failure(
                Error.Forbidden("Bid.SellerCannotBid", "Vendedor não pode dar lances no próprio leilão"));

        // 3. Validar valor mínimo
        var amountResult = Money.Create(amount, "BRL");
        if (!amountResult.IsSuccess)
            return Result.Failure(amountResult.Error);

        var minimumBid = auction.CurrentPrice.Add(auction.BidIncrement);
        if (!amountResult.Value.IsGreaterThanOrEqual(minimumBid).Value)
            return Result.Failure(
                Error.Validation("Bid.BidTooLow",
                    $"Lance deve ser no mínimo {minimumBid.Value} {minimumBid.Currency}"));

        // 4. Validar número máximo de lances por usuário
        if (auction.Rules.MaxBidsPerUser > 0)
        {
            var userBidCount = await _bidRepository.GetBidCountForUserInAuctionAsync(
                auction.Id,
                bidderId,
                cancellationToken);

            if (userBidCount >= auction.Rules.MaxBidsPerUser)
                return Result.Failure(
                    Error.Validation("Bid.MaxBidsExceeded",
                        $"Você atingiu o limite de {auction.Rules.MaxBidsPerUser} lances neste leilão"));
        }

        return Result.Success();
    }
}
