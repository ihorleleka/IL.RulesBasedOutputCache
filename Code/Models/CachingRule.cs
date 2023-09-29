using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IL.RulesBasedOutputCache.Models;

[Table("CachingRules")]
public record CachingRule
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public virtual Guid Id { get; } = Guid.NewGuid();
    [Required]
    public virtual required string RuleTemplate { get; set; }
    [Required]
    public virtual required RuleType RuleType { get; set; }
    [Required]
    public virtual required RuleAction RuleAction { get; set; }

    [Required]
    public virtual int Priority { get; set; }
    public virtual bool VaryByQueryString { get; set; }
    public virtual bool VaryByUser { get; set; }
    public virtual bool VaryByHost { get; set; }
    public virtual bool VaryByCulture { get; set; }
    public virtual TimeSpan? ResponseExpirationTimeSpan { get; set; }
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