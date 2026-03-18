namespace Auction.Application.Interfaces;

/// <summary>
/// Interface para serviço de cache distribuído
/// </summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    Task SetAsync<T>(string key, T value, TimeSpan expiry, CancellationToken cancellationToken = default);

    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    Task<long> IncrementAsync(string key, CancellationToken cancellationToken = default);

    Task<bool> AcquireLockAsync(string key, TimeSpan expiry, CancellationToken cancellationToken = default);

    Task ReleaseLockAsync(string key, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    Task<T?> GetOrSetAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan expiry,
        CancellationToken cancellationToken = default);
}
