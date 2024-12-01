using System.ComponentModel.DataAnnotations;
using IL.RulesBasedOutputCache.Middleware;
using IL.RulesBasedOutputCache.MinimalApi;
using IL.RulesBasedOutputCache.Persistence.Rules;
using IL.RulesBasedOutputCache.Persistence.Rules.Interfaces;
using IL.RulesBasedOutputCache.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace IL.RulesBasedOutputCache.Extensions;

public static class RulesBasedOutputCacheApplicationBuilderExtensions
{
    public static WebApplication UseRulesBasedOutputCache(this WebApplication app)
    {
        InitializeDatabase(app);
        var options = app.Services.GetRequiredService<IOptions<RulesBasedOutputCacheConfiguration>>().Value;
        app.MapAdminPanelEndpoints(options);
        app.MapAdminPanelApiEndpoints(options);
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
            var validationContext = new ValidationContext(rulesRepo);
            var validRules = cacheConfig.Value.CachingRules
                .Where(x => !x.Validate(validationContext).Any())
                .ToList();
            rulesRepo.AddRules(validRules).Wait();
        }
    }
}