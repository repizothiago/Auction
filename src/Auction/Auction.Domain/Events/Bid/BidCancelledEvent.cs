using Auction.SharedKernel;

namespace Auction.Domain.Events.Bid;

public record BidCancelledEvent(
    Guid BidId,
    Guid AuctionId,
    Guid BidderId) : DomainEvent;
