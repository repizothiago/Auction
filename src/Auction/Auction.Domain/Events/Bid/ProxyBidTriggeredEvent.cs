using Auction.SharedKernel;

namespace Auction.Domain.Events.Bid;

public sealed record ProxyBidTriggeredEvent(Guid BidId, Guid AuctionId, decimal NewAmount) : DomainEvent;
