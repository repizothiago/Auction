using Auction.SharedKernel;

namespace Auction.Domain.Events.Bid;

public sealed record BidAcceptedEvent(
    Guid BidId,
    Guid AuctionId,
    Guid BidderId,
    decimal Amount) : DomainEvent;
