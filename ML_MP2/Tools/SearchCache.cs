namespace ML_MP2.Tools;

/// <summary>
/// Simple in-memory cache for search results to avoid repeated API calls during testing.
/// Helps mitigate rate limiting issues with Semantic Scholar free tier.
/// </summary>
public class SearchCache
{
    private static readonly Dictionary<string, (string results, DateTime cachedAt)> _cache = new();
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Generates a cache key from search parameters.
    /// </summary>
    public static string GenerateKey(string query, int? minYear, int? maxYear, int minCitations)
    {
        return $"{query}|{minYear}|{maxYear}|{minCitations}";
    }

    /// <summary>
    /// Tries to get a cached result. Returns true if found and not expired.
    /// </summary>
    public static bool TryGetCached(string cacheKey, out string? result)
    {
        result = null;
        
        if (!_cache.ContainsKey(cacheKey))
            return false;

        var (cachedResult, cachedAt) = _cache[cacheKey];
        
        // Check if cache expired
        if (DateTime.UtcNow - cachedAt > CacheDuration)
        {
            _cache.Remove(cacheKey);
            return false;
        }

        result = cachedResult;
        return true;
    }

    /// <summary>
    /// Caches a search result.
    /// </summary>
    public static void Cache(string cacheKey, string result)
    {
        _cache[cacheKey] = (result, DateTime.UtcNow);
    }

    /// <summary>
    /// Clears all cached results.
    /// </summary>
    public static void ClearCache()
    {
        _cache.Clear();
    }

    /// <summary>
    /// Gets current cache size.
    /// </summary>
    public static int GetCacheSize()
    {
        return _cache.Count;
    }
}

