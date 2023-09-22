using IL.RulesBasedOutputCache.Persistence.Rules;
using IL.RulesBasedOutputCache.Persistence.Rules.Interfaces;
using IL.RulesBasedOutputCache.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IL.RulesBasedOutputCache.Extensions;

public static class RulesBasedOutputCacheServiceCollectionExtensions
{
    public static IServiceCollection AddRulesBasedOutputCache(this IServiceCollection services, IConfiguration config, Action<RulesBasedOutputCacheConfiguration>? setupOptions = null)
    {
        if (config.GetSection(Constants.Constants.ConfigurationSection).Exists())
        {
            services.Configure<RulesBasedOutputCacheConfiguration>(config.GetSection(Constants.Constants.ConfigurationSection));
        }
        else
        {
            services.Configure(setupOptions ?? RulesBasedOutputCacheConfiguration.Default);
        }
        services.AddOutputCache();
        services.AddSingleton<IRulesRepository, InMemoryRulesRepository>();

        return services;
    }
}