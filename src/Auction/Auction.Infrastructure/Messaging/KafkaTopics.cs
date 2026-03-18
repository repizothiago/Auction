namespace Auction.Infrastructure.Messaging;

/// <summary>
/// Tópicos do Kafka padronizados
/// </summary>
public static class KafkaTopics
{
    // Auction events
    public const string AuctionCreated = "auction-created";
    public const string AuctionStarted = "auction-started";
    public const string AuctionEnded = "auction-ended";
    public const string AuctionCancelled = "auction-cancelled";
    public const string AuctionExtended = "auction-extended";

    // Bid events
    public const string BidPlaced = "bid-placed";
    public const string BidAccepted = "bid-accepted";
    public const string BidRejected = "bid-rejected";
    public const string BidOutbid = "bid-outbid";
    public const string ProxyBidTriggered = "proxy-bid-triggered";

    // User events
    public const string UserRegistered = "user-registered";
}
