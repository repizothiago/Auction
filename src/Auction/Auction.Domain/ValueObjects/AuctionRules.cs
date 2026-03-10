namespace Auction.Domain.ValueObjects;

/// <summary>
/// Value Object que define as regras de um leilão
/// </summary>
public sealed record AuctionRules
{
    /// <summary>
    /// Tempo de extensão quando um lance é feito nos últimos minutos
    /// </summary>
    public TimeSpan ExtensionTime { get; init; }

    /// <summary>
    /// Janela de tempo antes do fim em que a extensão se aplica
    /// </summary>
    public TimeSpan ExtensionWindow { get; init; }

    /// <summary>
    /// Número máximo de lances por usuário (0 = ilimitado)
    /// </summary>
    public int MaxBidsPerUser { get; init; }

    /// <summary>
    /// Se permite lances automáticos (proxy bids)
    /// </summary>
    public bool AllowProxyBids { get; init; }

    /// <summary>
    /// Construtor para criação completa
    /// </summary>
    public AuctionRules(TimeSpan extensionTime, TimeSpan extensionWindow, int maxBidsPerUser, bool allowProxyBids)
    {
        ExtensionTime = extensionTime;
        ExtensionWindow = extensionWindow;
        MaxBidsPerUser = maxBidsPerUser;
        AllowProxyBids = allowProxyBids;
    }

    /// <summary>
    /// Construtor parameterless para desserialização EF Core JSON
    /// </summary>
    public AuctionRules()
    {
    }

    /// <summary>
    /// Cria regras padrão para leilões
    /// </summary>
    public static AuctionRules CreateDefault() => new(
        extensionTime: TimeSpan.FromMinutes(5),
        extensionWindow: TimeSpan.FromMinutes(5),
        maxBidsPerUser: 5,
        allowProxyBids: true
    );
}
