namespace Auction.Application.Queries.Auction;

/// <summary>
/// Query para obter todos os leilões do sistema
/// </summary>
public record GetAllAuctionsQuery(
    int PageNumber = 1,
    int PageSize = 10
);
