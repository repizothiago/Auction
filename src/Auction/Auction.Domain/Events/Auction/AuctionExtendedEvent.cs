using Auction.SharedKernel;

namespace Auction.Domain.Events.Auction;

public sealed record AuctionExtendedEvent(Guid AuctionId,DateTime NewEndDate) : DomainEvent;