using IL.RulesBasedOutputCache.Persistence.Rules;
using IL.RulesBasedOutputCache.Persistence.Rules.Interfaces;
using IL.RulesBasedOutputCache.Services;
using IL.RulesBasedOutputCache.Settings;
using IL.VirtualViews.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IL.RulesBasedOutputCache.Extensions;

public static class RulesBasedOutputCacheServiceCollectionExtensions
{
    /// <inheritdoc cref="RulesBasedOutputCacheWebAppBuilderExtensions.AddRulesBasedOutputCache" />
    public static IServiceCollection AddRulesBasedOutputCache(this IServiceCollection services, 
        IConfiguration config,
        Action<RulesBasedOutputCacheConfiguration>? setupOptions = null)
    {
        services.AddOutputCache();
        services.AddScoped<IRulesRepository, InMemoryRulesRepository>();
        var section = config.GetSection(Constants.Constants.ConfigurationSection);
        if (section.Exists())
        {
            services.Configure<RulesBasedOutputCacheConfiguration>(section);
            var rulesBasedAppInsightsConfiguration = section.Get<RulesBasedOutputCacheConfiguration>();
            if (!string.IsNullOrEmpty(rulesBasedAppInsightsConfiguration?.SqlConnectionStringName))
            {
                services.AddDbContext<SqlRulesRepository>(options => options
                    .UseSqlServer(config
                            .GetConnectionString(rulesBasedAppInsightsConfiguration.SqlConnectionStringName),
                        sqlServerOptions => sqlServerOptions.MigrationsHistoryTable("__RulesBasedOutputCacheMigrations")
                    )
                );
                services.AddScoped<IRulesRepository, SqlRulesRepository>();
            }

            if (rulesBasedAppInsightsConfiguration is { AdminPanel.AdminPanelEnabled: false })
            {
                return services;
            }

            AddServicesForAdminPanelRendering(services);
        }
        else
        {
            services.Configure(setupOptions ?? RulesBasedOutputCacheConfiguration.Default);
            if (setupOptions == null)
            {
                return services;
            }

            var configuration = new RulesBasedOutputCacheConfiguration();
            setupOptions(configuration);
            if (configuration is { AdminPanel.AdminPanelEnabled: false })
            {
                return services;
            }

            AddServicesForAdminPanelRendering(services);
        }

        return services;
    }

    private static void AddServicesForAdminPanelRendering(IServiceCollection services)
    {
        services.AddVirtualViewsCapabilities();
        services.AddControllersWithViews().AddRazorRuntimeCompilation();
        services.AddHttpContextAccessor();
        services.AddSingleton<IViewRenderService, ViewRenderService>();
    }
}