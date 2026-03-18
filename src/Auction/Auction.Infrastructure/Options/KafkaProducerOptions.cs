namespace Auction.Infrastructure.Options;

public class KafkaProducerOptions
{
    public int MessageTimeoutMs { get; set; } = 10000;
    public int RequestTimeoutMs { get; set; } = 30000;
    public bool EnableIdempotence { get; set; } = true;
    public int MaxInFlight { get; set; } = 5;
    public string Acks { get; set; } = "all";
}
