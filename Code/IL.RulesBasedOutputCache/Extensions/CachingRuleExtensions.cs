using IL.RulesBasedOutputCache.Models;

namespace IL.RulesBasedOutputCache.Extensions;

internal static class CachingRuleExtensions
{
    public static bool MatchesCurrentRequest(this CachingRule rule, string httpRequestPathValue)
    {

        switch (rule.RuleType)
        {
            case RuleType.Regex:
                return MatchWildcardPattern(httpRequestPathValue.AsSpan(), rule.RuleTemplate.AsSpan());
            
            case RuleType.ExactPath:
                return string.Equals(httpRequestPathValue, rule.RuleTemplate, StringComparison.InvariantCultureIgnoreCase);

            case RuleType.FileExtension:
            {
                var pathValue = httpRequestPathValue.AsSpan();
                var requestFileExtension = GetFileExtension(pathValue);
                var templateSpan = rule.RuleTemplate.AsSpan();

                var startIndex = 0;
                int separatorIndex;

                while ((separatorIndex = templateSpan[startIndex..].IndexOf(Constants.Constants.MatchingExtensionsSeparator)) >= 0)
                {
                    if (requestFileExtension.Equals(templateSpan.Slice(startIndex, separatorIndex), StringComparison.InvariantCultureIgnoreCase))
                    {
                        return true;
                    }

                    startIndex += separatorIndex + 1; // +1 to skip the separator
                }

                // Check the last extension after the final separator
                return requestFileExtension.Equals(templateSpan[startIndex..], StringComparison.InvariantCultureIgnoreCase);
            }
            default:
                return false;
        }
    }

    private static ReadOnlySpan<char> GetFileExtension(ReadOnlySpan<char> path)
    {
        var lastDot = path.LastIndexOf('.');
        return lastDot >= 0 ? path[lastDot..] : ReadOnlySpan<char>.Empty;
    }

    private static bool MatchWildcardPattern(ReadOnlySpan<char> input, ReadOnlySpan<char> pattern)
    {
        // Simple wildcard matcher implementation (prefix*, *suffix, pre*fix)
        // This handles basic cases without regex allocations

        if (pattern.IsEmpty)
        {
            return input.IsEmpty;
        }

        if (pattern.EndsWith("*".AsSpan()))
        {
            var prefix = pattern[..^1];
            return input.Length >= prefix.Length &&
                   input[..prefix.Length].Equals(prefix, StringComparison.InvariantCultureIgnoreCase);
        }

        if (pattern.StartsWith("*".AsSpan()))
        {
            var suffix = pattern[1..];
            return input.Length >= suffix.Length &&
                   input[^suffix.Length..].Equals(suffix, StringComparison.InvariantCultureIgnoreCase);
        }

        var starIndex = pattern.IndexOf('*');
        if (starIndex >= 0)
        {
            var prefix = pattern[..starIndex];
            var suffix = pattern[(starIndex + 1)..];

            return input.Length >= prefix.Length + suffix.Length &&
                   input[..prefix.Length].Equals(prefix, StringComparison.InvariantCultureIgnoreCase) &&
                   input[^suffix.Length..].Equals(suffix, StringComparison.InvariantCultureIgnoreCase);
        }

        // No wildcards, just do exact match
        return input.Equals(pattern, StringComparison.InvariantCultureIgnoreCase);
    }

    public static int GetPriority(this CachingRule rule) => (int)rule.RuleAction * 10 + (int)rule.RuleType;
}