using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using ZiyoMarket.Service.Interfaces;

namespace ZiyoMarket.Service.Services;

/// <summary>
/// In-memory cache service implementation
/// </summary>
public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<CacheService> _logger;
    private static readonly ConcurrentDictionary<string, byte> _cacheKeys = new();

    public CacheService(IMemoryCache cache, ILogger<CacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public Task<T?> GetAsync<T>(string key) where T : class
    {
        try
        {
            if (_cache.TryGetValue(key, out T? value))
            {
                _logger.LogDebug("Cache HIT: {Key}", key);
                return Task.FromResult(value);
            }

            _logger.LogDebug("Cache MISS: {Key}", key);
            return Task.FromResult<T?>(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache key: {Key}", key);
            return Task.FromResult<T?>(null);
        }
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        try
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(5),
                SlidingExpiration = TimeSpan.FromMinutes(2)
            };

            // Register callback to remove key from tracking when evicted
            options.RegisterPostEvictionCallback((k, v, r, s) =>
            {
                _cacheKeys.TryRemove(k.ToString()!, out _);
            });

            _cache.Set(key, value, options);
            _cacheKeys.TryAdd(key, 0);

            _logger.LogDebug("Cache SET: {Key} (Expiration: {Expiration})", key, expiration);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache key: {Key}", key);
            return Task.CompletedTask;
        }
    }

    public Task RemoveAsync(string key)
    {
        try
        {
            _cache.Remove(key);
            _cacheKeys.TryRemove(key, out _);
            _logger.LogDebug("Cache REMOVE: {Key}", key);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache key: {Key}", key);
            return Task.CompletedTask;
        }
    }

    public Task RemoveByPrefixAsync(string prefix)
    {
        try
        {
            var keysToRemove = _cacheKeys.Keys.Where(k => k.StartsWith(prefix)).ToList();

            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
                _cacheKeys.TryRemove(key, out _);
            }

            _logger.LogDebug("Cache REMOVE BY PREFIX: {Prefix} ({Count} keys)", prefix, keysToRemove.Count);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache by prefix: {Prefix}", prefix);
            return Task.CompletedTask;
        }
    }

    public Task<bool> ExistsAsync(string key)
    {
        return Task.FromResult(_cache.TryGetValue(key, out _));
    }
}
