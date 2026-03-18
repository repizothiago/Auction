using Auction.Application.DTOs;
using Auction.Application.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace Auction.Application.Queries.Bid;

/// <summary>
/// Handler para buscar lances de um leilão (paginado)
/// </summary>
public class GetBidsByAuctionQueryHandler
{
    private readonly IBidRepository _bidRepository;
    private readonly ILogger<GetBidsByAuctionQueryHandler> _logger;

    public GetBidsByAuctionQueryHandler(
        IBidRepository bidRepository,
        ILogger<GetBidsByAuctionQueryHandler> logger)
    {
        _bidRepository = bidRepository;
        _logger = logger;
    }

    public async Task<List<BidDto>> HandleAsync(
        GetBidsByAuctionQuery query,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Fetching bids for auction {AuctionId}, page {PageNumber}, size {PageSize}",
            query.AuctionId, query.PageNumber, query.PageSize);

        var bids = await _bidRepository.GetByAuctionIdAsync(
            query.AuctionId,
            query.PageNumber,
            query.PageSize,
            cancellationToken);

        return bids.Select(bid => new BidDto(
            bid.Id,
            bid.AuctionId,
            bid.BidderId,
            bid.Amount.Value,
            bid.Amount.Currency,
            bid.IsAutoBid,
            bid.BidStatus.ToString(),
            bid.BidTime)).ToList();
    }
}
