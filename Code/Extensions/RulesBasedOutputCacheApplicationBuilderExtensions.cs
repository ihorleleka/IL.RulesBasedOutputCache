using IL.RulesBasedOutputCache.Middleware;
using Microsoft.AspNetCore.Builder;

namespace IL.RulesBasedOutputCache.Extensions;

public static class RulesBasedOutputCacheApplicationBuilderExtensions
{
    public static IApplicationBuilder UseRulesBasedOutputCache(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RulesBasedOutputCacheMiddleware>();
    }
}