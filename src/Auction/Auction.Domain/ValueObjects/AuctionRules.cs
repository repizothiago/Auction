namespace Auction.Domain.ValueObjects;

public sealed record AuctionRules
{
    public AuctionRules(TimeSpan extensionTime, TimeSpan extensionWindow, int maxBidsPerUser, bool allowProxyBids)
    {
        ExtensionTime = extensionTime;
        ExtensionWindow = extensionWindow;
        MaxBidsPerUser = maxBidsPerUser;
        AllowProxyBids = allowProxyBids;
    }

    public TimeSpan ExtensionTime { get; }
    public TimeSpan ExtensionWindow { get; }
    public int MaxBidsPerUser { get; }
    public bool AllowProxyBids { get; }

    public static AuctionRules CreateDefault() => new(
        extensionTime: TimeSpan.FromMinutes(5),
        extensionWindow: TimeSpan.FromMinutes(5),
        maxBidsPerUser: 5,
        allowProxyBids: true
    );

}
