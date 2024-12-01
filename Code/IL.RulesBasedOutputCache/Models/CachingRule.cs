using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IL.RulesBasedOutputCache.Models;

[Table("CachingRules")]
public sealed record CachingRule : IValidatableObject
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; } = Guid.NewGuid();
    [Required]
    public required string RuleTemplate { get; init; }
    [Required]
    public required RuleType RuleType { get; init; }
    [Required]
    public required RuleAction RuleAction { get; init; }

    [Required]
    public int Priority { get; set; }
    public bool VaryByQueryString { get; init; }
    public bool VaryByUser { get; init; }
    public bool VaryByHost { get; init; }
    public bool VaryByCulture { get; init; }
    public TimeSpan? ResponseExpirationTimeSpan { get; init; }
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var validationResults = new List<ValidationResult>();
        if (string.IsNullOrEmpty(RuleTemplate))
        {
            validationResults.Add(new ValidationResult("RuleTemplate cannot be empty."));
        }
        switch (RuleType)
        {
            case RuleType.FileExtension:
                var matchingExtensions = RuleTemplate.Split(Constants.Constants.MatchingExtensionsSeparator);
                if (matchingExtensions.Any(x => string.IsNullOrEmpty(Path.GetExtension(x))))
                {
                    validationResults.Add(new ValidationResult($"RuleTemplate has invalid value for given RuleType: {Enum.GetName(RuleType)}."));
                }
                break;

            case RuleType.ExactPath:
                if (!RuleTemplate.StartsWith('/') || RuleTemplate.Contains('*'))
                {
                    validationResults.Add(new ValidationResult($"RuleTemplate has invalid value for given RuleType: {Enum.GetName(RuleType)}."));
                }
                break;

            case RuleType.Regex:
                if (!(RuleTemplate.StartsWith("/") && RuleTemplate.Contains('*')))
                {
                    validationResults.Add(new ValidationResult($"RuleTemplate has invalid value for given RuleType: {Enum.GetName(RuleType)}."));
                }
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(validationContext));
        }

        return validationResults;
    }
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