using Auction.Domain.ValueObjects;

namespace Auction.Application.Commands.Bid;

public record PlaceBidCommand(
    Guid AuctionId,
    Guid BidderId,
    Money Bid,
    string IdempotencyKey);
