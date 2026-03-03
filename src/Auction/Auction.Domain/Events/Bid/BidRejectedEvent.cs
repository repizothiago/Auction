using Auction.SharedKernel;

namespace Auction.Domain.Events.Bid;

public sealed record BidRejectedEvent(Guid BidId, Guid AuctionId, string Reason) : DomainEvent;
