using Auction.Domain.Entities;

namespace Auction.Application.Interfaces.Repositories;

public interface IBidRepository
{
    Task<Bid?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Bid>> GetByAuctionIdAsync(Guid auctionId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<List<Bid>> GetByBidderIdAsync(Guid bidderId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<Bid?> GetHighestBidForAuctionAsync(Guid auctionId, CancellationToken cancellationToken = default);
    Task<int> GetBidCountForUserInAuctionAsync(Guid auctionId, Guid bidderId, CancellationToken cancellationToken = default);
    Task AddAsync(Bid bid, CancellationToken cancellationToken = default);
    void Update(Bid bid);
}
