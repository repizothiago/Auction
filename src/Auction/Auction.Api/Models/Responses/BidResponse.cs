namespace Auction.Api.Models.Responses;

public record BidResponse(
    Guid Id,
    Guid AuctionId,
    Guid BidderId,
    decimal Amount,
    string Currency,
    bool IsAutoBid,
    string Status,
    DateTime BidTime);
