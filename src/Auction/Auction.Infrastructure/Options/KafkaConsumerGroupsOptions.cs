namespace Auction.Infrastructure.Options;

public class KafkaConsumerGroupsOptions
{
    public string AuctionService { get; set; } = "auction-service-group";
    public string NotificationService { get; set; } = "notification-service-group";
    public string AnalyticsService { get; set; } = "analytics-service-group";
}
