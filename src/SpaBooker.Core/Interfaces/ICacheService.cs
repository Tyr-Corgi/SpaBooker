namespace SpaBooker.Core.Interfaces;

public interface ICacheService
{
    /// <summary>
    /// Get a cached value or compute and cache it
    /// </summary>
    Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);
    
    /// <summary>
    /// Get a cached value
    /// </summary>
    Task<T?> GetAsync<T>(string key);
    
    /// <summary>
    /// Set a cached value
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    
    /// <summary>
    /// Remove a cached value
    /// </summary>
    Task RemoveAsync(string key);
    
    /// <summary>
    /// Remove all cached values matching a pattern
    /// </summary>
    Task RemoveByPrefixAsync(string prefix);
    
    /// <summary>
    /// Check if a key exists in cache
    /// </summary>
    Task<bool> ExistsAsync(string key);
}

