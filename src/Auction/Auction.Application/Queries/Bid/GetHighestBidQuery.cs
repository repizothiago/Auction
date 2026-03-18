namespace Auction.Application.Queries.Bid;

/// <summary>
/// Query para buscar o maior lance de um leilão
/// </summary>
public record GetHighestBidQuery(Guid AuctionId);
