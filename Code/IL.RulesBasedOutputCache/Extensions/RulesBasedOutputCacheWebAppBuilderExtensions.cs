using IL.RulesBasedOutputCache.Settings;
using Microsoft.AspNetCore.Builder;

namespace IL.RulesBasedOutputCache.Extensions;

public static class RulesBasedOutputCacheWebAppBuilderExtensions
{
    /// <summary>
    /// Adds required services and configurations for RulesBasedOutputCache. Requires .AddMvc() or .AddControllersWithViews() if admin panel should be used.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="config"></param>
    /// <param name="setupOptions"></param>
    /// <returns></returns>
    public static WebApplicationBuilder AddRulesBasedOutputCache(this WebApplicationBuilder builder, Action<RulesBasedOutputCacheConfiguration>? setupOptions = null)
    {
        builder.Services.AddRulesBasedOutputCache(builder.Configuration);
        return builder;
    }
}