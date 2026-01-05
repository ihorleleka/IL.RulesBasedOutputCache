using IL.Misc.Concurrency;
using IL.RulesBasedOutputCache.Extensions;
using IL.RulesBasedOutputCache.Models;
using IL.RulesBasedOutputCache.Persistence.Rules.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IL.RulesBasedOutputCache.Persistence.Rules;

internal sealed class SqlRulesRepository : DbContext, IRulesRepository
{
    private const string LockKey = "SqlRulesAccess";
    private const int RefreshRateInMinutes = 5;
    private static DateTime? _lastUpdated;
    private static List<CachingRule>? _cachedValues;

    private DbSet<CachingRule> CachingRules { get; set; }

    private DbSet<CacheMetadata> CacheMetadata { get; set; }

    //dotnet ef --startup-project ./Code/IL.RulesBasedOutputCache/IL.RulesBasedOutputCache.csproj migrations add TimeSpanToString --context SqlRulesRepository --output-dir Migrations --project ./Code/IL.RulesBasedOutputCache/IL.RulesBasedOutputCache.csproj
    //Uncomment for migrations purpose
    // public SqlRulesRepository()
    // {
    // }
    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    // {
    //     optionsBuilder.UseSqlServer();
    //     base.OnConfiguring(optionsBuilder);
    // }
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
        if (AreCachedRulesValid(null))
        {
            return _cachedValues!;
        }

        using (await LockManager.GetLockAsync(LockKey))
        {
            if (AreCachedRulesValid(null))
            {
                return _cachedValues!;
            }

            var metadata = await CacheMetadata.AsNoTracking().FirstOrDefaultAsync();
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
        _lastUpdated = DateTime.UtcNow;
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

    private static bool AreCachedRulesValid(CacheMetadata? metadata) =>
        _cachedValues != null &&
        (_lastUpdated != null && (DateTime.UtcNow - _lastUpdated).Value.Minutes <= RefreshRateInMinutes
         ||
         metadata != null
         && DateTime.UtcNow > metadata.LastUpdated);
}