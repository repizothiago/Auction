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
