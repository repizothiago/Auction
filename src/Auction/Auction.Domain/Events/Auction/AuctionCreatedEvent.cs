using Auction.SharedKernel;

namespace Auction.Domain.Events.Auction;

public sealed record AuctionCreatedEvent(
    Guid AuctionId,
    string Title,
    decimal StartingPrice) : DomainEvent;