namespace Auction.Application.Commands.Auction;

/// <summary>
/// Comando para cancelar um leilão
/// </summary>
public sealed record CancelAuctionCommand(Guid AuctionId, string Reason);
