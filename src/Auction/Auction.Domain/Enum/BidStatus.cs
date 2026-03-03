namespace Auction.Domain.Enum;

public enum BidStatus
{
    Pending = 0,    // Aguardando validação
    Accepted = 1,   // Aceito como lance válido
    Rejected = 2,   // Rejeitado (valor inválido, leilão encerrado, etc.)
    Outbid = 3,     // Superado por outro lance
    Winning = 4,    // Lance vencedor atual
    Won = 5,        // Venceu o leilão
    Lost = 6        // Perdeu o leilão
}
