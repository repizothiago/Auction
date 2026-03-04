namespace Auction.Infrastructure.Options;

public class KafkaOptions
{
    public const string SectionName = "Kafka";

    public string BootstrapServers { get; set; } = string.Empty;
    public KafkaTopicsOptions Topics { get; set; } = new();
    public KafkaConsumerGroupsOptions ConsumerGroups { get; set; } = new();
    public KafkaProducerOptions Producer { get; set; } = new();
    public KafkaConsumerOptions Consumer { get; set; } = new();
}

public class KafkaTopicsOptions
{
    public string AuctionCreated { get; set; } = "auction.created";
    public string AuctionStarted { get; set; } = "auction.started";
    public string AuctionEnded { get; set; } = "auction.ended";
    public string AuctionCancelled { get; set; } = "auction.cancelled";
    public string AuctionExtended { get; set; } = "auction.extended";
    public string BidPlaced { get; set; } = "bid.placed";
}

public class KafkaConsumerGroupsOptions
{
    public string AuctionService { get; set; } = "auction-service-group";
    public string NotificationService { get; set; } = "notification-service-group";
    public string AnalyticsService { get; set; } = "analytics-service-group";
}

public class KafkaProducerOptions
{
    public int MessageTimeoutMs { get; set; } = 10000;
    public int RequestTimeoutMs { get; set; } = 30000;
    public bool EnableIdempotence { get; set; } = true;
    public int MaxInFlight { get; set; } = 5;
    public string Acks { get; set; } = "all";
}

public class KafkaConsumerOptions
{
    public string AutoOffsetReset { get; set; } = "earliest";
    public bool EnableAutoCommit { get; set; } = false;
    public int MaxPollIntervalMs { get; set; } = 300000;
    public int SessionTimeoutMs { get; set; } = 10000;
}
