using Auction.SharedKernel;

namespace Auction.Domain.Events.Bid;

public sealed record BidPlacedEvent(
    Guid BidId,
    Guid AuctionId,
    Guid BidderId,
    decimal Amount,
    bool IsAutoBid,
    DateTime BidTime) : DomainEvent;
