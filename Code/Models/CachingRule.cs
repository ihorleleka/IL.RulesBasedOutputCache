namespace IL.RulesBasedOutputCache.Models;

public record CachingRule
{
    public required string RuleTemplate { get; set; }
    public required RuleType RuleType { get; set; }
    public required RuleAction RuleAction { get; set; }
    public bool VaryByQueryString { get; set; }
    public TimeSpan? ResponseExpirationTimeSpan { get; set; }
}

public enum RuleType
{
    Regex,
    ExactPath,
    FileExtension
}

public enum RuleAction
{
    Disallow,
    Allow
}