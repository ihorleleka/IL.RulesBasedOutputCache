using IL.Misc.Concurrency;
using IL.RulesBasedOutputCache.Extensions;
using IL.RulesBasedOutputCache.Models;
using IL.RulesBasedOutputCache.Persistence.Rules.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace IL.RulesBasedOutputCache.Persistence.Rules;

internal class RulesSqlRepository : DbContext, IRulesRepository
{
    private const string CacheKey = "CachingRules";
    private readonly IMemoryCache _cache;
    private DbSet<CachingRule> CachingRules { get; set; }

    public RulesSqlRepository(DbContextOptions<RulesSqlRepository> options, IMemoryCache memoryCache) : base(options)
    {
        _cache = memoryCache;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CachingRule>().HasKey(x => x.Id);
    }

    public async Task<List<CachingRule>> GetAll()
    {
        using (await LockManager.GetLockAsync(CacheKey))
        {
            if (_cache.TryGetValue(CacheKey, out List<CachingRule>? cachedRules) && cachedRules != null)
            {
                return cachedRules!;
            }

            var rules = await CachingRules.OrderByDescending(r => r.Priority).ToListAsync();
            _cache.Set(CacheKey, rules, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
                SlidingExpiration = TimeSpan.FromMinutes(10)
            });
            return rules;
        }
    }

    public async Task AddRule(CachingRule rule)
    {
        using (await LockManager.GetLockAsync(CacheKey))
        {
            rule.Priority = rule.GetPriority();
            CachingRules.Add(rule);
            await SaveChangesAsync();
            _cache.Remove(CacheKey);
        }
    }

    public async Task AddRules(List<CachingRule> rules)
    {
        using (await LockManager.GetLockAsync(CacheKey))
        {
            foreach (var cachingRule in rules)
            {
                cachingRule.Priority = cachingRule.GetPriority();
            }

            CachingRules.AddRange(rules);
            await SaveChangesAsync();
            _cache.Remove(CacheKey);
        }
    }

    public async Task DeleteRuleById(Guid id)
    {
        using (await LockManager.GetLockAsync(CacheKey))
        {
            var rule = await CachingRules.FirstOrDefaultAsync(r => r.Id == id);
            if (rule != null)
            {
                CachingRules.Remove(rule);
                await SaveChangesAsync();
                _cache.Remove(CacheKey);
            }
        }
    }
}