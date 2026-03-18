using Auction.Application.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace Auction.Infrastructure.Caching;

public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public RedisCacheService(
        IConnectionMultiplexer redis,
        ILogger<RedisCacheService> logger)
    {
        _redis = redis;
        _logger = logger;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var value = await db.StringGetAsync(key);

            if (value.IsNullOrEmpty)
            {
                _logger.LogDebug("Cache miss for key: {Key}", key);
                return default;
            }

            _logger.LogDebug("Cache hit for key: {Key}", key);
            return JsonSerializer.Deserialize<T>(value.ToString(), _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache key: {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var serialized = JsonSerializer.Serialize(value, _jsonOptions);

            await db.StringSetAsync(key, serialized, expiry);

            _logger.LogDebug("Cache set for key: {Key}, expiry: {Expiry}", key, expiry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(key);

            _logger.LogDebug("Cache removed for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache key: {Key}", key);
        }
    }

    public async Task<long> IncrementAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var value = await db.StringIncrementAsync(key);

            _logger.LogDebug("Cache incremented for key: {Key}, new value: {Value}", key, value);

            return value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing cache key: {Key}", key);
            throw;
        }
    }

    public async Task<bool> AcquireLockAsync(string key, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var lockValue = Guid.NewGuid().ToString();

            // SET key value NX PX milliseconds
            var acquired = await db.StringSetAsync(key, lockValue, expiry, When.NotExists);

            if (acquired)
                _logger.LogDebug("Lock acquired for key: {Key}", key);
            else
                _logger.LogDebug("Failed to acquire lock for key: {Key}", key);

            return acquired;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acquiring lock for key: {Key}", key);
            return false;
        }
    }

    public async Task ReleaseLockAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(key);

            _logger.LogDebug("Lock released for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error releasing lock for key: {Key}", key);
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            return await db.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence of key: {Key}", key);
            return false;
        }
    }

    public async Task<T?> GetOrSetAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan expiry,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Tentar obter do cache
            var cached = await GetAsync<T>(key, cancellationToken);
            if (cached != null)
                return cached;

            // Cache miss - executar factory
            _logger.LogDebug("Cache miss for key: {Key}, executing factory", key);
            var value = await factory();

            // Salvar no cache
            if (value != null)
                await SetAsync(key, value, expiry, cancellationToken);

            return value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetOrSetAsync for key: {Key}", key);

            // Em caso de erro de cache, tentar executar factory diretamente
            return await factory();
        }
    }
}

/// <summary>
/// Chaves de cache padronizadas
/// </summary>
public static class CacheKeys
{
    // Auction cache keys
    public static string Auction(Guid auctionId) => $"auction:{auctionId}";
    public static string ActiveAuctions(int page, int pageSize, Guid? categoryId) =>
        $"auctions:active:page:{page}:size:{pageSize}:category:{categoryId}";
    public static string AuctionSequence(Guid auctionId) => $"auction:{auctionId}:sequence";

    // User cache keys
    public static string User(Guid userId) => $"user:{userId}";
    public static string UserByEmail(string email) => $"user:email:{email.ToLower()}";

    // Category cache keys
    public static string Category(Guid categoryId) => $"category:{categoryId}";
    public static string AllCategories => "categories:all";

    // Lock keys (Distributed Locks)
    public static string AuctionLock(Guid auctionId) => $"lock:auction:{auctionId}";
    public static string BidProcessingLock(Guid bidId) => $"lock:bid:{bidId}";
}
