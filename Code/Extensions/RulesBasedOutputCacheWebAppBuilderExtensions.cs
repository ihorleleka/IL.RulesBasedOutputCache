using IL.RulesBasedOutputCache.Persistence.Rules;
using IL.RulesBasedOutputCache.Persistence.Rules.Interfaces;
using IL.RulesBasedOutputCache.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IL.RulesBasedOutputCache.Extensions;

public static class RulesBasedOutputCacheWebAppBuilderExtensions
{
    public static WebApplicationBuilder AddRulesBasedOutputCache(this WebApplicationBuilder builder, Action<RulesBasedOutputCacheConfiguration>? setupOptions = null)
    {
        if (builder.Configuration.GetSection(Constants.Constants.ConfigurationSection).Exists())
        {
            builder.Services.Configure<RulesBasedOutputCacheConfiguration>(builder.Configuration.GetSection(Constants.Constants.ConfigurationSection));
        }
        else
        {
            builder.Services.Configure(setupOptions ?? RulesBasedOutputCacheConfiguration.Default);
        }
        builder.Services.AddOutputCache();
        builder.Services.AddSingleton<IRulesRepository, InMemoryRulesRepository>();

        return builder;
    }
}