using IL.RulesBasedOutputCache.Extensions;
using IL.RulesBasedOutputCache.Models;
using IL.RulesBasedOutputCache.Persistence.Rules.Interfaces;
using IL.RulesBasedOutputCache.Settings;
using Microsoft.Extensions.Options;

namespace IL.RulesBasedOutputCache.Persistence.Rules;

internal class InMemoryRulesRepository : IRulesRepository
{
    private static SortedSet<CachingRule>? _rules;

    public InMemoryRulesRepository(IOptions<RulesBasedOutputCacheConfiguration> cacheConfiguration)
    {
        _rules ??= new SortedSet<CachingRule>(cacheConfiguration.Value.CachingRules,
            Comparer<CachingRule>.Create((a, b) => b.GetPriority().CompareTo(a.GetPriority())));
    }

    public Task<List<CachingRule>> GetAll() => Task.FromResult(_rules!.ToList());

    public Task AddRule(CachingRule rule)
    {
        _rules!.Add(rule);
        return Task.CompletedTask;
    }

    public async Task AddRules(List<CachingRule> rules)
    {
        foreach (var cachingRule in rules)
        {
            await AddRule(cachingRule);
        }
    }

    public Task DeleteRuleById(Guid id)
    {
        if (_rules!.FirstOrDefault(x => x.Id == id) is { } rule)
        {
            _rules!.Remove(rule);
        }

        return Task.CompletedTask;
    }
}