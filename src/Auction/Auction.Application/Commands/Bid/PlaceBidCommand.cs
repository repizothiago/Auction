namespace Auction.Application.Commands.Bid;

public record PlaceBidCommand(
    Guid AuctionId,
    Guid BidderId,
    decimal Amount,
    string IdempotencyKey);
