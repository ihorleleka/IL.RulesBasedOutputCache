using System.ComponentModel.DataAnnotations;
using IL.RulesBasedOutputCache.Extensions;
using IL.RulesBasedOutputCache.Models;
using IL.RulesBasedOutputCache.Persistence.Rules.Interfaces;
using IL.RulesBasedOutputCache.Settings;
using Microsoft.Extensions.Options;

namespace IL.RulesBasedOutputCache.Persistence.Rules;

internal sealed class InMemoryRulesRepository : IRulesRepository
{
    private static SortedSet<CachingRule>? _rules;

    public InMemoryRulesRepository(IOptions<RulesBasedOutputCacheConfiguration> cacheConfiguration)
    {
        var validationContext = new ValidationContext(this);
        var validRules = cacheConfiguration.Value.CachingRules
            .Where(x => !x.Validate(validationContext).Any());
        _rules ??= new SortedSet<CachingRule>(validRules,
            Comparer<CachingRule>.Create((a, b) =>
            {
                var comparisonResult = b.GetPriority().CompareTo(a.GetPriority());
                //if rules are of same priority compare them by template - prevents duplicates
                return comparisonResult == 0 ? string.Compare(a.RuleTemplate, b.RuleTemplate, StringComparison.InvariantCultureIgnoreCase) : comparisonResult;
            }));
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