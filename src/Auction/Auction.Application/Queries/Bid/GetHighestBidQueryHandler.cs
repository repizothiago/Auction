using Auction.Application.DTOs;
using Auction.Application.Interfaces.Repositories;
using Auction.SharedKernel;
using Auction.SharedKernel.Errors;
using Microsoft.Extensions.Logging;

namespace Auction.Application.Queries.Bid;

/// <summary>
/// Handler para buscar o maior lance de um leilão
/// </summary>
public class GetHighestBidQueryHandler
{
    private readonly IBidRepository _bidRepository;
    private readonly ILogger<GetHighestBidQueryHandler> _logger;

    public GetHighestBidQueryHandler(
        IBidRepository bidRepository,
        ILogger<GetHighestBidQueryHandler> logger)
    {
        _bidRepository = bidRepository;
        _logger = logger;
    }

    public async Task<Result<BidDto>> HandleAsync(
        GetHighestBidQuery query,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[Consulta] Buscando maior lance do leilão: AuctionId={AuctionId}", query.AuctionId);

        var bid = await _bidRepository.GetHighestBidForAuctionAsync(
            query.AuctionId,
            cancellationToken);

        if (bid is null)
        {
            return Result<BidDto>.Failure(
                Error.NotFound(
                    "Bid.NoBidsFound",
                    $"Nenhum lance ativo encontrado para o leilão {query.AuctionId}"));
        }

        var dto = new BidDto(
            bid.Id,
            bid.AuctionId,
            bid.BidderId,
            bid.Amount.Value,
            bid.Amount.Currency,
            bid.IsAutoBid,
            bid.BidStatus.ToString(),
            bid.BidTime);

        return Result<BidDto>.Success(dto);
    }
}
