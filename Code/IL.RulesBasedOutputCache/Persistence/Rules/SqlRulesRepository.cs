using IL.Misc.Concurrency;
using IL.RulesBasedOutputCache.Extensions;
using IL.RulesBasedOutputCache.Models;
using IL.RulesBasedOutputCache.Persistence.Rules.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace IL.RulesBasedOutputCache.Persistence.Rules;

internal sealed class SqlRulesRepository : DbContext, IRulesRepository
{
    private const string LockKey = "SqlRulesAccess";
    private List<CachingRule>? _cachedValues;

    private DbSet<CachingRule> CachingRules { get; set; }

    private DbSet<CacheMetadata> CacheMetadata { get; set; }

    public SqlRulesRepository(DbContextOptions<SqlRulesRepository> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CachingRule>().HasKey(x => x.Id);
        modelBuilder.Entity<CacheMetadata>().HasKey(x => x.Id);
    }

    public async Task<List<CachingRule>> GetAll()
    {
        var metadata = await CacheMetadata.AsNoTracking().FirstOrDefaultAsync();
        if (AreCachedRulesValid(metadata))
        {
            return _cachedValues!;
        }

        using (await LockManager.GetLockAsync(LockKey))
        {
            metadata = await CacheMetadata.AsNoTracking().FirstOrDefaultAsync();
            if (AreCachedRulesValid(metadata))
            {
                return _cachedValues!;
            }

            _cachedValues = await CachingRules
                .OrderByDescending(r => r.Priority)
                .ToListAsync();
            return _cachedValues;
        }
    }

    public async Task AddRule(CachingRule rule)
    {
        //prevent from introducing duplicate entries
        if (await CachingRules.AnyAsync(x => x.RuleTemplate == rule.RuleTemplate && x.Priority == rule.GetPriority()))
        {
            return;
        }

        using (await LockManager.GetLockAsync(LockKey))
        {
            rule.Priority = rule.GetPriority();
            CachingRules.Add(rule);
            await SetOrUpdateMetadata();
            await SaveChangesAsync();
        }
    }

    private async Task SetOrUpdateMetadata()
    {
        var metadata = await CacheMetadata.FirstOrDefaultAsync();
        if (metadata == null)
        {
            CacheMetadata.Add(new CacheMetadata { LastUpdated = DateTime.UtcNow });
        }
        else
        {
            metadata.LastUpdated = DateTime.UtcNow;
        }
    }

    public async Task AddRules(List<CachingRule> rules)
    {
        using (await LockManager.GetLockAsync(LockKey))
        {
            foreach (var cachingRule in rules)
            {
                cachingRule.Priority = cachingRule.GetPriority();
            }

            CachingRules.AddRange(rules);
            await SetOrUpdateMetadata();
            await SaveChangesAsync();
        }
    }

    public async Task DeleteRuleById(Guid id)
    {
        using (await LockManager.GetLockAsync(LockKey))
        {
            var rule = await CachingRules.FirstOrDefaultAsync(r => r.Id == id);
            if (rule != null)
            {
                CachingRules.Remove(rule);
                await SetOrUpdateMetadata();
                await SaveChangesAsync();
            }
        }
    }

    private bool AreCachedRulesValid(CacheMetadata? metadata) =>
        _cachedValues != null &&
        metadata != null
        && DateTime.UtcNow > metadata.LastUpdated;
}