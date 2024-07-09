using IL.RulesBasedOutputCache.Models;

namespace IL.RulesBasedOutputCache.Settings;

public sealed class RulesBasedOutputCacheConfiguration
{
    public static Action<RulesBasedOutputCacheConfiguration> Default = _ => { };
    public bool OutputCacheEnabled { get; init; }
    public bool AdminPanelEnabled { get; init; }
    public bool AdminPanelApiEnabled { get; init; }
    public required TimeSpan DefaultCacheTimeout { get; init; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// The largest cacheable size for the response body in bytes. The default is set to 64 MB.
    /// </summary>
    public long MaximumBodySize { get; init; } = 64 * 1024 * 1024;

    public List<CachingRule> CachingRules { get; init; } = new();

    public string? SqlConnectionStringName { get; init; }
}