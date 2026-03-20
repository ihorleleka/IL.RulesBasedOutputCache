using System.Buffers;
using IL.RulesBasedOutputCache.Middleware;
using IL.RulesBasedOutputCache.Persistence.Rules;
using IL.RulesBasedOutputCache.Persistence.Rules.Interfaces;
using IL.RulesBasedOutputCache.Services;
using IL.RulesBasedOutputCache.Settings;
using IL.VirtualViews.Extensions;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IL.RulesBasedOutputCache.Extensions;

public static class RulesBasedOutputCacheServiceCollectionExtensions
{
    /// <inheritdoc cref="RulesBasedOutputCacheWebAppBuilderExtensions.AddRulesBasedOutputCache" />
    public static IServiceCollection AddRulesBasedOutputCache(this IServiceCollection services,
        IConfiguration config,
        Action<RulesBasedOutputCacheConfiguration>? setupOptions = null)
    {
        var section = config.GetSection(Constants.Constants.ConfigurationSection);
        ConfigureRulesBasedOutputCacheOptions(services, section, setupOptions);

        var configuration = BuildConfiguration(section, setupOptions);
        if (!configuration.OutputCacheEnabled)
        {
            services.TryAddSingleton<IOutputCacheStore, NoopOutputCacheStore>();
            return services;
        }

        services.AddOutputCache();
        services.AddScoped<RulesBasedOutputCacheMiddleware>();
        services.AddScoped<IRulesRepository, InMemoryRulesRepository>();

        if (!string.IsNullOrEmpty(configuration.SqlConnectionStringName))
        {
            services.AddDbContext<SqlRulesRepository>(options => options
                .UseSqlServer(config
                        .GetConnectionString(configuration.SqlConnectionStringName),
                    sqlServerOptions => sqlServerOptions.MigrationsHistoryTable("__RulesBasedOutputCacheMigrations")
                )
            );
            services.AddScoped<IRulesRepository, SqlRulesRepository>();
        }

        TryConfigureRedisStore(services, config, configuration);

        if (!configuration.AdminPanel.AdminPanelEnabled)
        {
            return services;
        }

        AddServicesForAdminPanelRendering(services);
        return services;
    }

    private static void ConfigureRulesBasedOutputCacheOptions(IServiceCollection services,
        IConfigurationSection section,
        Action<RulesBasedOutputCacheConfiguration>? setupOptions)
    {
        if (section.Exists())
        {
            services.Configure<RulesBasedOutputCacheConfiguration>(section);
        }
        else
        {
            services.Configure(RulesBasedOutputCacheConfiguration.Default);
        }

        if (setupOptions != null)
        {
            services.PostConfigure(setupOptions);
        }
    }

    private static RulesBasedOutputCacheConfiguration BuildConfiguration(IConfigurationSection section,
        Action<RulesBasedOutputCacheConfiguration>? setupOptions)
    {
        var configuration = section.Exists()
            ? section.Get<RulesBasedOutputCacheConfiguration>() ?? new RulesBasedOutputCacheConfiguration()
            : new RulesBasedOutputCacheConfiguration();

        setupOptions?.Invoke(configuration);
        return configuration;
    }

    private static void TryConfigureRedisStore(IServiceCollection services,
        IConfiguration config,
        RulesBasedOutputCacheConfiguration configuration)
    {
        if (!string.IsNullOrEmpty(configuration.RedisConnectionStringName))
        {
            services.AddStackExchangeRedisOutputCache(options =>
            {
                options.Configuration = config.GetConnectionString(configuration.RedisConnectionStringName);
                options.InstanceName = configuration.RedisInstanceName;
            });
        }
    }

    private static void AddServicesForAdminPanelRendering(IServiceCollection services)
    {
        services.AddVirtualViewsCapabilities();
        services.AddControllersWithViews().AddRazorRuntimeCompilation();
        services.AddHttpContextAccessor();
        services.AddSingleton<IViewRenderService, ViewRenderService>();
    }

    private sealed class NoopOutputCacheStore : IOutputCacheStore
    {
        public ValueTask<byte[]?> GetAsync(string key, CancellationToken cancellationToken) =>
            ValueTask.FromResult<byte[]?>(null);

        public ValueTask SetAsync(string key,
            byte[] value,
            string[]? tags,
            TimeSpan validFor,
            CancellationToken cancellationToken) =>
            ValueTask.CompletedTask;

        public ValueTask SetAsync(string key,
            ReadOnlySequence<byte> value,
            ReadOnlyMemory<string> tags,
            TimeSpan validFor,
            CancellationToken cancellationToken) =>
            ValueTask.CompletedTask;

        public ValueTask EvictByTagAsync(string tag, CancellationToken cancellationToken) =>
            ValueTask.CompletedTask;
    }
}
