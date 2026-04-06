using System;
using System.Threading.Tasks;

namespace ZiyoMarket.Service.Interfaces;

/// <summary>
/// Cache service for improving API performance
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Get cached value by key
    /// </summary>
    Task<T?> GetAsync<T>(string key) where T : class;

    /// <summary>
    /// Set cache value with expiration
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;

    /// <summary>
    /// Remove cached value by key
    /// </summary>
    Task RemoveAsync(string key);

    /// <summary>
    /// Remove all cached values matching pattern (e.g., "products:*")
    /// </summary>
    Task RemoveByPrefixAsync(string prefix);

    /// <summary>
    /// Check if key exists in cache
    /// </summary>
    Task<bool> ExistsAsync(string key);
}
