namespace Auction.Application.Interfaces;

/// <summary>
/// Unit of Work pattern para transações ACID
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<bool> SaveChangesAndReturnStatusAsync(CancellationToken cancellationToken = default);
}
