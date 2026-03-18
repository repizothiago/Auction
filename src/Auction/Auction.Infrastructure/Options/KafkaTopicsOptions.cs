namespace Auction.Infrastructure.Options;

public class KafkaTopicsOptions
{
    public string AuctionCreated { get; set; } = "auction.created";
    public string AuctionStarted { get; set; } = "auction.started";
    public string AuctionEnded { get; set; } = "auction.ended";
    public string AuctionCancelled { get; set; } = "auction.cancelled";
    public string AuctionExtended { get; set; } = "auction.extended";
    public string BidPlaced { get; set; } = "bid.placed";
}
