using IL.RulesBasedOutputCache.Extensions;
using IL.RulesBasedOutputCache.Persistence.Rules.Interfaces;
using IL.RulesBasedOutputCache.Services;
using IL.RulesBasedOutputCache.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IL.RulesBasedOutputCache.MinimalApi;

public static class AdminPanelEndpoints
{
    public static WebApplication MapAdminPanelEndpoints(this WebApplication app, RulesBasedOutputCacheConfiguration options)
    {
        if (options.AdminPanel.AdminPanelEnabled is false)
        {
            return app;
        }

        app.MapGet(options.AdminPanel.AdminPanelUrl.TrimStart('/'),
            async ([FromServices] IHttpContextAccessor httpContextAccessor,
                [FromServices] IRulesRepository rulesRepository,
                [FromServices] IViewRenderService viewRenderService) =>
            {
                httpContextAccessor.HttpContext?.IgnoreRulesBasedOutputCacheForCurrentRequest();
                var rules = await rulesRepository.GetAll();
                var content = await viewRenderService.RenderViewToStringAsync(Constants.Constants.AdminPanelViewPath, rules);
                return Results.Content(content, "text/html");
            });
        return app;
    }
}