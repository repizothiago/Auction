using Auction.Domain.ValueObjects;

namespace Auction.Api.Models.Requests;

public record PlaceBidRequest(
    Money Bid,
    string IdempotencyKey);
