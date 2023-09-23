using IL.RulesBasedOutputCache.Extensions;
using IL.RulesBasedOutputCache.Models;
using IL.RulesBasedOutputCache.Persistence.Rules.Interfaces;
using IL.RulesBasedOutputCache.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace IL.RulesBasedOutputCache.Persistence.Rules;

internal class RulesSqlRepository : DbContext, IRulesRepository
{
    private DbSet<CachingRule> CachingRules { get; set; }

    public RulesSqlRepository(DbContextOptions<RulesSqlRepository> options, IOptions<RulesBasedOutputCacheConfiguration> cacheConfiguration) : base(options)
    {
        Database.Migrate();
        if (!CachingRules.Any())
        {
            AddRules(cacheConfiguration.Value.CachingRules).Wait();
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CachingRule>().HasKey(x => x.Id);
    }

    public async Task<List<CachingRule>> GetAll()
    {
        return await CachingRules.OrderByDescending(r => r.Priority).ToListAsync();
    }

    public async Task AddRule(CachingRule rule)
    {
        rule.Priority = rule.GetPriority();
        CachingRules.Add(rule);
        await SaveChangesAsync();
    }

    public async Task AddRules(List<CachingRule> rules)
    {
        foreach (var cachingRule in rules)
        {
            cachingRule.Priority = cachingRule.GetPriority();
        }
        CachingRules.AddRange(rules);
        await SaveChangesAsync();
    }

    public async Task DeleteRuleById(Guid id)
    {
        var rule = await CachingRules.FirstOrDefaultAsync(r => r.Id == id);
        if (rule != null)
        {
            CachingRules.Remove(rule);
            await SaveChangesAsync();
        }
    }
}