using IL.RulesBasedOutputCache.Extensions;
using IL.RulesBasedOutputCache.Models;
using IL.RulesBasedOutputCache.Persistence.Rules.Interfaces;
using IL.RulesBasedOutputCache.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace IL.RulesBasedOutputCache.MinimalApi;

public static class AdminPanelApiEndpoints
{
    public static RouteGroupBuilder MapAdminPanelApiEndpoints(this WebApplication app, RulesBasedOutputCacheConfiguration options)
    {

        var apiGroping = app
            .MapGroup(Constants.Constants.AdminPanelApiUrlBasePath)
            .DisableAntiforgery();

        apiGroping.MapGet("getAll",
            async ([FromServices] IHttpContextAccessor httpContextAccessor, [FromServices] IRulesRepository rulesRepository) =>
            {
                httpContextAccessor.HttpContext?.IgnoreRulesBasedOutputCacheForCurrentRequest();
                return Results.Ok(await rulesRepository.GetAll());
            });
        apiGroping.MapGet("evictAllCacheEntries",
            async ([FromServices] IHttpContextAccessor httpContextAccessor, [FromServices] IOutputCacheStore cacheStore) =>
            {
                httpContextAccessor.HttpContext?.IgnoreRulesBasedOutputCacheForCurrentRequest();
                await cacheStore.EvictByTagAsync(Constants.Constants.OutputCacheSharedTag, httpContextAccessor.HttpContext!.RequestAborted);
                return Results.Ok("All cache entries are evicted!");
            });
        apiGroping.MapPost("addRule",
            async ([FromServices] IHttpContextAccessor httpContextAccessor,
                [FromServices] IRulesRepository rulesRepository,
                [FromServices] IOptions<RulesBasedOutputCacheConfiguration> outputCacheOptions,
                [FromForm] CachingRule rule) =>
            {
                httpContextAccessor.HttpContext?.IgnoreRulesBasedOutputCacheForCurrentRequest();
                await rulesRepository.AddRule(rule);
                return Results.Redirect(outputCacheOptions.Value.AdminPanel.AdminPanelUrl);
            });
        apiGroping.MapPost("deleteRule",
            async ([FromServices] IHttpContextAccessor httpContextAccessor,
                [FromServices] IRulesRepository rulesRepository,
                [FromServices] IOptions<RulesBasedOutputCacheConfiguration> outputCacheOptions,
                [FromForm] Guid ruleId) =>
            {
                httpContextAccessor.HttpContext?.IgnoreRulesBasedOutputCacheForCurrentRequest();
                await rulesRepository.DeleteRuleById(ruleId);
                return Results.Redirect(outputCacheOptions.Value.AdminPanel.AdminPanelUrl);
            });

        return apiGroping;
    }
}