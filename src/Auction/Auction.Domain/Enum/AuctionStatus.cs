namespace Auction.Domain.Enum;

public enum AuctionStatus
{
    Draft = 0,           // Rascunho (não visível)
    Scheduled = 1,       // Agendado para início
    Active = 2,          // Ativo (recebendo lances)
    Paused = 3,          // Pausado temporariamente
    Ended = 4,           // Finalizado (sem vencedor se não atingiu reserve)
    Cancelled = 5,       // Cancelado
    AwaitingPayment = 6, // Aguardando pagamento do vencedor
    Completed = 7        // Completo (pago e entregue)
}
