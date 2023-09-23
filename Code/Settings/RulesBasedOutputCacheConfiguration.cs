using IL.RulesBasedOutputCache.Models;

namespace IL.RulesBasedOutputCache.Settings;

public class RulesBasedOutputCacheConfiguration
{
    public static Action<RulesBasedOutputCacheConfiguration> Default = _ => { };
    public bool OutputCacheEnabled { get; set; } = false;

    public bool OutputCacheAdminPanelEnabled { get; set; }

    public required TimeSpan DefaultCacheTimeout { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// The largest cacheable size for the response body in bytes. The default is set to 64 MB.
    /// </summary>
    public long MaximumBodySize { get; set; } = 64 * 1024 * 1024;

    public List<CachingRule> CachingRules { get; set; } = new();

    public string? SqlConnectionStringName { get; set; }
}