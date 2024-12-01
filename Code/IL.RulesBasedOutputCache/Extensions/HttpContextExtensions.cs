using IL.RulesBasedOutputCache.Models.Interfaces;
using Microsoft.AspNetCore.Http;

namespace IL.RulesBasedOutputCache.Extensions;

public static class HttpContextExtensions
{
    public static void ApplyCustomOutputCacheTagToCurrentRequest(this HttpContext context, string tag)
    {
        if (context.Features.Get<IRulesBasedOutputCacheFeature>() is { } outputCacheFeature)
        {
            outputCacheFeature.Tags.Add(tag);
        }
    }

    public static void ApplyCustomOutputCacheTagsToCurrentRequest(this HttpContext context, HashSet<string> tags)
    {
        if (context.Features.Get<IRulesBasedOutputCacheFeature>() is { } outputCacheFeature)
        {
            outputCacheFeature.Tags.UnionWith(tags);
        }
    }

    public static void IgnoreRulesBasedOutputCacheForCurrentRequest(this HttpContext context)
    {
        if (context.Features.Get<IRulesBasedOutputCacheFeature>() is { } outputCacheFeature)
        {
            outputCacheFeature.UseOutputCaching = false;
        }
    }
}