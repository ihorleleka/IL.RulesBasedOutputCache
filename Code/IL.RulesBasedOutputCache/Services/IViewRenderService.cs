namespace IL.RulesBasedOutputCache.Services;

public interface IViewRenderService
{
    Task<string> RenderViewToStringAsync(string viewPath, object model);
}