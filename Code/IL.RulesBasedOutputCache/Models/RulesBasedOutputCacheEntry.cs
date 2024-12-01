using Microsoft.AspNetCore.Http;

namespace IL.RulesBasedOutputCache.Models;

internal sealed class RulesBasedOutputCacheEntry
{
    /// <summary>
    /// Gets the created date and time of the cache entry.
    /// </summary>
    internal DateTimeOffset Created { get; init; }

    /// <summary>
    /// Gets the status code of the cache entry.
    /// </summary>
    internal int StatusCode { get; init; }

    /// <summary>
    /// Gets the headers of the cache entry.
    /// </summary>
    internal HeaderDictionary? Headers { get; set; }

    /// <summary>
    /// Gets the body of the cache entry.
    /// </summary>
    internal RulesBasedCacheCachedResponseBody? Body { get; set; }

    /// <summary>
    /// Gets the tags of the cache entry.
    /// </summary>
    internal string[] Tags { get; init; } = [];
}