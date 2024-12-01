using Microsoft.AspNetCore.Http;

namespace IL.RulesBasedOutputCache.Models.Interfaces;

public interface IRulesBasedOutputCacheFeature
{
    /// <summary>
    /// Determines whether the output caching logic should be configured for the incoming HTTP request.
    /// </summary>
    public bool UseOutputCaching { get; set; }

    public HttpContext HttpContext { get; init; }

    public HashSet<string> Tags { get; }
}