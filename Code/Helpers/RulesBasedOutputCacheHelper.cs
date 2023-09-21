using IL.RulesBasedOutputCache.Models.Interfaces;
using Microsoft.AspNetCore.Http;

namespace IL.RulesBasedOutputCache.Helpers;

public static class RulesBasedOutputCacheHelper
{
    public static void ApplyCustomCacheTagToCurrentRequest(this HttpContext context, string tag)
    {
        if (context.Features.Get<IRulesBasedOutputCacheFeature>() is { } outputCacheFeature)
        {
            outputCacheFeature.Tags.Add(tag);
        }
    }

    public static void ApplyCustomCacheTagsToCurrentRequest(this HttpContext context, HashSet<string> tags)
    {
        if (context.Features.Get<IRulesBasedOutputCacheFeature>() is { } outputCacheFeature)
        {
            outputCacheFeature.Tags.UnionWith(tags);
        }
    }
}