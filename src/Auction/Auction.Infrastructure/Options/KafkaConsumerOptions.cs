namespace Auction.Infrastructure.Options;

public class KafkaConsumerOptions
{
    public string AutoOffsetReset { get; set; } = "earliest";
    public bool EnableAutoCommit { get; set; } = false;
    public int MaxPollIntervalMs { get; set; } = 300000;
    public int SessionTimeoutMs { get; set; } = 10000;
}
