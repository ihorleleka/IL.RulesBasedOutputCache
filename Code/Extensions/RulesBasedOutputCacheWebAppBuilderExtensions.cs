using IL.RulesBasedOutputCache.Settings;
using Microsoft.AspNetCore.Builder;

namespace IL.RulesBasedOutputCache.Extensions;

public static class RulesBasedOutputCacheWebAppBuilderExtensions
{
    public static WebApplicationBuilder AddRulesBasedOutputCache(this WebApplicationBuilder builder, Action<RulesBasedOutputCacheConfiguration>? setupOptions = null)
    {
        builder.Services.AddRulesBasedOutputCache(builder.Configuration);
        return builder;
    }
}