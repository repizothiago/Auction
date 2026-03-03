namespace Auction.Application.Interfaces.Repositories;

public interface ICategoryRepository
{
    Task<Domain.Entities.Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<Domain.Entities.Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    
    Task<List<Domain.Entities.Category>> GetAllAsync(CancellationToken cancellationToken = default);
    
    Task AddAsync(Domain.Entities.Category category, CancellationToken cancellationToken = default);
    
    void Update(Domain.Entities.Category category);
}
