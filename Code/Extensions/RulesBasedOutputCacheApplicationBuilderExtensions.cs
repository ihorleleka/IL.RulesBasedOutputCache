using IL.RulesBasedOutputCache.Middleware;
using IL.RulesBasedOutputCache.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace IL.RulesBasedOutputCache.Extensions;

public static class RulesBasedOutputCacheApplicationBuilderExtensions
{
    public static IApplicationBuilder UseRulesBasedOutputCache(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            if (string.IsNullOrEmpty(context.Request.Path.Value)
                || !context.Request.Path.Value.StartsWith($"/{Constants.Constants.AdminPanelUrlBasePath}", StringComparison.InvariantCultureIgnoreCase))
            {
                await next();
                return;
            }

            var cacheConfig = context.RequestServices.GetRequiredService<IOptions<RulesBasedOutputCacheConfiguration>>();
            if (cacheConfig.Value.AutomatedCacheAdminPanelEnabled
                && !string.IsNullOrEmpty(context.Request.Path.Value)
                && context.Request.Path.Value.StartsWith($"/{Constants.Constants.AdminPanelUrlBasePath}", StringComparison.InvariantCultureIgnoreCase))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            await next();
        });
        app.UseMiddleware<RulesBasedOutputCacheMiddleware>();
        return app;
    }
}