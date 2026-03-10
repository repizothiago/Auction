using Auction.SharedKernel;

namespace Auction.Domain.Events.Bid;

/// <summary>
/// Evento publicado quando um lance é solicitado (antes do processamento)
/// Usado para processamento assíncrono no consumer
/// </summary>
public sealed record BidPlacementRequestedEvent(
    Guid BidId,
    Guid AuctionId,
    Guid BidderId,
    decimal Amount,
    string Currency,
    bool IsAutoBid,
    string IdempotencyKey,
    DateTime RequestedAt) : DomainEvent;
