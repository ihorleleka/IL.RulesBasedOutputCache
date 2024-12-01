using IL.RulesBasedOutputCache.Models;

namespace IL.RulesBasedOutputCache.Persistence.Rules.Interfaces;

public interface IRulesRepository
{
    Task<List<CachingRule>> GetAll();

    Task AddRule(CachingRule rule);

    Task AddRules(List<CachingRule> rules);

    Task DeleteRuleById(Guid id);
}