namespace Auction.Application.Queries.Bid;

/// <summary>
/// Query para buscar um lance por ID
/// </summary>
public record GetBidByIdQuery(Guid BidId);
