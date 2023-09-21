using IL.RulesBasedOutputCache.Extensions;
using IL.RulesBasedOutputCache.Models;
using IL.RulesBasedOutputCache.Persistence.Rules.Interfaces;
using IL.RulesBasedOutputCache.Settings;
using Microsoft.Extensions.Options;

namespace IL.RulesBasedOutputCache.Persistence.Rules;

public class InMemoryRulesRepository : IRulesRepository
{
    private readonly SortedSet<CachingRule> _rules;

    public InMemoryRulesRepository(IOptions<RulesBasedOutputCacheConfiguration> cacheConfiguration)
    {
        _rules = new SortedSet<CachingRule>(cacheConfiguration.Value.CachingRules,
            Comparer<CachingRule>.Create((a, b) => a.GetPriority().CompareTo(b.GetPriority())));
    }

    public Task<List<CachingRule>> GetAll() => Task.FromResult(_rules.ToList());

    public Task AddRule(CachingRule rule)
    {
        _rules.Add(rule);
        return Task.CompletedTask;
    }

    public Task DeleteRuleById(Guid id)
    {
        if (_rules.FirstOrDefault(x => x.Id == id) is { } rule)
        {
            _rules.Remove(rule);
        }

        return Task.CompletedTask;
    }
}