using Auction.SharedKernel;

namespace Auction.Domain.Events.Auction;

public sealed record AuctionStartedEvent(Guid AuctionId) : DomainEvent;
