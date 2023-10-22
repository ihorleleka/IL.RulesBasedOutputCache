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
    public required string RuleTemplate { get; set; }
    [Required]
    public required RuleType RuleType { get; set; }
    [Required]
    public required RuleAction RuleAction { get; set; }

    [Required]
    public int Priority { get; set; }
    public bool VaryByQueryString { get; set; }
    public bool VaryByUser { get; set; }
    public bool VaryByHost { get; set; }
    public bool VaryByCulture { get; set; }
    public TimeSpan? ResponseExpirationTimeSpan { get; set; }
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
                if (!RuleTemplate.StartsWith('/') && !RuleTemplate.Contains('*'))
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
                throw new ArgumentOutOfRangeException();
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