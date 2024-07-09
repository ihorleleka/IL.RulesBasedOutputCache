using Il.ClassViewRendering.Extensions;
using IL.RulesBasedOutputCache.Models;
using IL.RulesBasedOutputCache.Persistence.Rules;
using IL.RulesBasedOutputCache.Persistence.Rules.Interfaces;
using IL.RulesBasedOutputCache.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IL.RulesBasedOutputCache.Extensions;

public static class RulesBasedOutputCacheServiceCollectionExtensions
{
    /// <inheritdoc cref="RulesBasedOutputCacheWebAppBuilderExtensions.AddRulesBasedOutputCache" />
    public static IServiceCollection AddRulesBasedOutputCache(this IServiceCollection services, IConfiguration config,
        Action<RulesBasedOutputCacheConfiguration>? setupOptions = null)
    {
        services.AddOutputCache();
        services.AddMemoryCache();
        services.AddScoped<IRulesRepository, InMemoryRulesRepository>();
        var section = config.GetSection(Constants.Constants.ConfigurationSection);
        if (section.Exists())
        {
            services.Configure<RulesBasedOutputCacheConfiguration>(section);
            var rulesBasedAppInsightsConfiguration = section.Get<RulesBasedOutputCacheConfiguration>();
            if (!string.IsNullOrEmpty(rulesBasedAppInsightsConfiguration?.SqlConnectionStringName))
            {
                services.AddDbContext<SqlRulesRepository>(options => options.UseSqlServer(config.GetConnectionString(rulesBasedAppInsightsConfiguration.SqlConnectionStringName)));
                services.AddScoped<IRulesRepository, SqlRulesRepository>();
            }

            if (rulesBasedAppInsightsConfiguration is { AdminPanelEnabled: true })
            {
                services.AddVirtualViewsCapabilities();
            }
        }
        else
        {
            services.Configure(setupOptions ?? RulesBasedOutputCacheConfiguration.Default);
            if (setupOptions != default)
            {
                var configuration = new RulesBasedOutputCacheConfiguration
                {
                    OutputCacheEnabled = false,
                    AdminPanelEnabled = false,
                    AdminPanelApiEnabled = false,
                    DefaultCacheTimeout = default,
                    MaximumBodySize = 0,
                    CachingRules = new List<CachingRule>(),
                    SqlConnectionStringName = null
                };
                setupOptions(configuration);
                if (configuration.AdminPanelEnabled)
                {
                    services.AddVirtualViewsCapabilities();
                }
            }
        }

        return services;
    }
}