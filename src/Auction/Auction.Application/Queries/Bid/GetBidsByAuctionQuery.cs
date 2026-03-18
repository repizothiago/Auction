namespace Auction.Application.Queries.Bid;

/// <summary>
/// Query para buscar lances de um leilão
/// </summary>
public record GetBidsByAuctionQuery(
    Guid AuctionId,
    int PageNumber = 1,
    int PageSize = 20);
