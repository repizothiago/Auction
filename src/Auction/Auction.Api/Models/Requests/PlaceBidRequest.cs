namespace Auction.Api.Models.Requests;

public record PlaceBidRequest(
    decimal Amount,
    string IdempotencyKey);
