using Auction.Domain.ValueObjects;
using Auction.SharedKernel;

namespace Auction.Domain.Events.Auction;

public sealed record AuctionEndedEvent(
    Guid AuctionId,
    Guid? WinnerId,
    bool HasWinner,
    Money? WinningBid) : DomainEvent;
