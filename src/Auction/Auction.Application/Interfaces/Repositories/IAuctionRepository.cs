namespace Auction.Application.Interfaces.Repositories;

public interface IAuctionRepository
{
    Task<Domain.Entities.Auction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<Domain.Entities.Auction>> GetAllAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<List<Domain.Entities.Auction>> GetActiveAuctionsAsync(
        int pageNumber,
        int pageSize,
        Guid? categoryId,
        CancellationToken cancellationToken = default);
    
    Task<int> GetActiveAuctionsCountAsync(
        Guid? categoryId,
        CancellationToken cancellationToken = default);
    
    Task<List<Domain.Entities.Auction>> GetScheduledAuctionsReadyToStartAsync(
        CancellationToken cancellationToken = default);
    
    Task<List<Domain.Entities.Auction>> GetExpiredActiveAuctionsAsync(
        CancellationToken cancellationToken = default);
    
    Task<List<Domain.Entities.Auction>> GetBySellerIdAsync(
        Guid sellerId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
    
    Task AddAsync(Domain.Entities.Auction auction, CancellationToken cancellationToken = default);
    
    void Update(Domain.Entities.Auction auction);
    
    void Remove(Domain.Entities.Auction auction);
}
