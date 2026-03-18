namespace Auction.Application.DTOs;

/// <summary>
/// DTO para retornar dados de um lance
/// </summary>
public record BidDto(
    Guid Id,
    Guid AuctionId,
    Guid BidderId,
    decimal Amount,
    string Currency,
    bool IsAutoBid,
    string Status,
    DateTime BidTime);
