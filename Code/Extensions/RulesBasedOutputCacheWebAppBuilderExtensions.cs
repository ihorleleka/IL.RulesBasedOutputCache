using IL.RulesBasedOutputCache.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IL.RulesBasedOutputCache.Extensions;

public static class RulesBasedOutputCacheWebAppBuilderExtensions
{
    public const string ConfigurationSection = "RulesBasedOutputCache";

    public static WebApplicationBuilder AddRulesBasedOutputCache(this WebApplicationBuilder builder, Action<RulesBasedOutputCacheConfiguration>? setupOptions = null)
    {
        if (builder.Configuration.GetSection(ConfigurationSection).Exists())
        {
            builder.Services.Configure<RulesBasedOutputCacheConfiguration>(builder.Configuration.GetSection(ConfigurationSection));
        }
        else
        {
            builder.Services.Configure(setupOptions ?? RulesBasedOutputCacheConfiguration.Default);
        }
        builder.Services.AddOutputCache();

        return builder;
    }
}