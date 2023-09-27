using IL.RulesBasedOutputCache.Middleware;
using IL.RulesBasedOutputCache.Persistence.Rules;
using IL.RulesBasedOutputCache.Persistence.Rules.Interfaces;
using IL.RulesBasedOutputCache.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace IL.RulesBasedOutputCache.Extensions;

public static class RulesBasedOutputCacheApplicationBuilderExtensions
{
    /// <summary>
    /// Adds a RulesBasedOutputCacheMiddleware type to the application's request pipeline. Requires .UseRouting() and .MapControllers() for correct functioning.
    /// </summary>
    /// <param name="app">The <see cref="IApplicationBuilder"/> instance.</param>
    /// <returns>The <see cref="IApplicationBuilder"/> instance.</returns>
    public static IApplicationBuilder UseRulesBasedOutputCache(this IApplicationBuilder app)
    {
        InitializeDatabase(app);
        app.Use(async (context, next) =>
        {
            if (string.IsNullOrEmpty(context.Request.Path.Value)
                || !context.Request.Path.Value.StartsWith($"/{Constants.Constants.AdminPanelUrlBasePath}", StringComparison.InvariantCultureIgnoreCase))
            {
                await next();
                return;
            }

            var cacheConfig = context.RequestServices.GetRequiredService<IOptions<RulesBasedOutputCacheConfiguration>>();
            if (!(cacheConfig.Value.OutputCacheEnabled && cacheConfig.Value.OutputCacheAdminPanelEnabled)
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

    private static void InitializeDatabase(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var rulesRepo = scope.ServiceProvider.GetRequiredService<IRulesRepository>();
        if (rulesRepo is SqlRulesRepository sqlRepository)
        {
            sqlRepository.Database.Migrate();
        }

        var cacheConfig = scope.ServiceProvider.GetRequiredService<IOptions<RulesBasedOutputCacheConfiguration>>();
        if (!rulesRepo.GetAll().Result.Any())
        {
            rulesRepo.AddRules(cacheConfig.Value.CachingRules).Wait();
        }
    }
}