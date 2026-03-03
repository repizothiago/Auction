using Auction.SharedKernel;

namespace Auction.Domain.Events.Auction;

public sealed record AuctionCancelledEvent(Guid AuctionId, string Reason) : DomainEvent;
