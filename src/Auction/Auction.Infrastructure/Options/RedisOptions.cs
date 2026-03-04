namespace Auction.Infrastructure.Options;

public class RedisOptions
{
    public const string SectionName = "Redis";

    public string ConnectionString { get; set; } = string.Empty;
    public bool AbortOnConnectFail { get; set; } = false;
    public int ConnectTimeoutMs { get; set; } = 5000;
    public int SyncTimeoutMs { get; set; } = 5000;
    public int AsyncTimeoutMs { get; set; } = 5000;
    public string Password { get; set; } = string.Empty;
}
