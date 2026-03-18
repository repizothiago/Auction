using Auction.SharedKernel;

namespace Auction.Domain.Events.Bid;

public sealed record BidOutbidEvent(
    Guid BidId,
    Guid AuctionId,
    Guid BidderId,
    decimal PreviousAmount) : DomainEvent;