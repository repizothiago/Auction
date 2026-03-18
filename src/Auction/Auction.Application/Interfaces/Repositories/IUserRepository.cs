namespace Auction.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<Domain.Entities.User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Domain.Entities.User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task AddAsync(Domain.Entities.User user, CancellationToken cancellationToken = default);

    void Update(Domain.Entities.User user);
}
