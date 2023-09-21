using IL.RulesBasedOutputCache.Models;

namespace IL.RulesBasedOutputCache.Settings;

public class RulesBasedOutputCacheConfiguration
{
    public static Action<RulesBasedOutputCacheConfiguration> Default = _ => { };
    public bool AutomatedCacheEnabled { get; set; } = false;
    public required TimeSpan DefaultCacheTimeout { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// The largest cacheable size for the response body in bytes. The default is set to 64 MB.
    /// </summary>
    public long MaximumBodySize { get; set; } = 64 * 1024 * 1024;

    public List<CachingRule> CachingRules { get; set; } = new();
}