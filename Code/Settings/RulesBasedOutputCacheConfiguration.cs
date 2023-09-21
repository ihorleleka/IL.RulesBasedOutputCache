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

    public List<CachingRule> ProcessedCachingRules()
    {
        _processedCachingRules ??= new Lazy<List<CachingRule>>(() =>
        {
            return CachingRules
                .OrderBy(x => x.RuleAction)
                .ThenBy(x => x.RuleType)
                .ToList();
        });
        return _processedCachingRules.Value;
    }

    private Lazy<List<CachingRule>>? _processedCachingRules;
}