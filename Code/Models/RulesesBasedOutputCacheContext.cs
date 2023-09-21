using IL.RulesBasedOutputCache.Models.Interfaces;
using IL.RulesBasedOutputCache.StreamExt;
using Microsoft.AspNetCore.Http;

namespace IL.RulesBasedOutputCache.Models;

internal class RulesesBasedOutputCacheContext : IRulesBasedOutputCacheFeature
{
    /// <summary>
    /// Determines whether the output caching logic should be configured for the incoming HTTP request.
    /// </summary>
    public bool UseOutputCaching { get; set; }

    public required HttpContext HttpContext { get; init; }

    public HashSet<string> Tags { get; } = new();

    internal string CacheKey { get; set; } = default!;

    internal Stream OriginalResponseStream { get; set; } = default!;
    internal RulesBasedOutputCacheStream OutputCacheStream { get; set; } = default!;
    internal bool ResponseStarted { get; set; }
    internal DateTimeOffset? ResponseTime { get; set; }
    internal TimeSpan? ResponseExpirationTimeSpan { get; set; }
    internal TimeSpan CachedResponseValidFor { get; set; }
    internal RulesBasedOutputCacheEntry CachedResponse { get; set; }
    internal TimeSpan CachedEntryAge { get; set; }
    internal bool IsCacheEntryFresh { get; set; }
}