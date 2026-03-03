namespace Auction.Domain.Enum;

public enum AuctionType
{
    Standard = 0,  // Leilão inglês (preço ascendente)
    Dutch = 1,     // Leilão holandês (preço descendente)
    Sealed = 2,    // Lances selados (sem visibilidade)
    Reverse = 3    // Leilão reverso (menor preço ganha)
}
