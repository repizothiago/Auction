using Auction.SharedKernel;

namespace Auction.Application.Interfaces;

/// <summary>
/// Interface para publicação de eventos em Message Bus (Kafka)
/// </summary>
public interface IMessageBus
{
    Task PublishAsync<TEvent>(
        string topic,
        TEvent @event,
        string partitionKey,
        CancellationToken cancellationToken = default) where TEvent : IDomainEvent;
    
    Task PublishBatchAsync<TEvent>(
        string topic,
        IEnumerable<TEvent> events,
        Func<TEvent, string> partitionKeySelector,
        CancellationToken cancellationToken = default) where TEvent : IDomainEvent;
}

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
