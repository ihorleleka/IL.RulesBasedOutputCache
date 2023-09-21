using System.Text.RegularExpressions;
using IL.RulesBasedOutputCache.Models;
using Microsoft.AspNetCore.Http;

namespace IL.RulesBasedOutputCache.Extensions;

internal static class CachingRuleExtensions
{
    public static bool MatchesCurrentRequest(this CachingRule rule, HttpContext httpContext)
    {
        switch (rule.RuleType)
        {
            case RuleType.Regex:
                {
                    var regexPattern = "^" + Regex.Escape(rule.RuleTemplate).Replace("\\*", ".*") + "$";
                    return Regex.IsMatch(httpContext.Request.Path.Value!, regexPattern, RegexOptions.IgnoreCase);
                }
            case RuleType.ExactPath:
                return string.Equals(httpContext.Request.Path.Value, rule.RuleTemplate, StringComparison.InvariantCultureIgnoreCase);

            case RuleType.FileExtension:
                {
                    var requestFileExtension = Path.GetExtension(httpContext.Request.Path.Value);
                    return string.Equals(requestFileExtension, rule.RuleTemplate, StringComparison.InvariantCultureIgnoreCase);
                }
            default:
                return false;
        }
    }

    public static int GetPriority(this CachingRule rule) => (int)rule.RuleAction * 10 + (int)rule.RuleType;
}