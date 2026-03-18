namespace Auction.Infrastructure.Messaging.Contracts;

/// <summary>
/// DTO para deserializar mensagem do Kafka (bid-placement-requested)
/// </summary>
public record BidPlacementRequest(
    Guid BidId,
    Guid AuctionId,
    Guid BidderId,
    decimal Amount,
    string Currency,
    bool IsAutoBid,
    string IdempotencyKey,
    DateTime RequestedAt);
