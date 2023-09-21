using IL.RulesBasedOutputCache.Models;

namespace IL.RulesBasedOutputCache.Persistence.Rules.Interfaces;

public interface IRulesRepository
{
    Task<List<CachingRule>> GetAll();

    Task AddRule(CachingRule rule);

    Task DeleteRuleById(Guid id);
}