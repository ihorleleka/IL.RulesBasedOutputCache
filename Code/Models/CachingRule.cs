namespace IL.RulesBasedOutputCache.Models;

public record CachingRule
{
    public Guid Id { get; } = Guid.NewGuid();
    public required string RuleTemplate { get; set; }
    public required RuleType RuleType { get; set; }
    public required RuleAction RuleAction { get; set; }
    public bool VaryByQueryString { get; set; }
    public TimeSpan? ResponseExpirationTimeSpan { get; set; }
}

public enum RuleType
{
    FileExtension,
    ExactPath,
    Regex
}

public enum RuleAction
{
    Allow,
    Disallow
}