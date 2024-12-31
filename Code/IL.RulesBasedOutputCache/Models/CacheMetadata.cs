using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IL.RulesBasedOutputCache.Models;

[Table("CacheMetadata")]
public sealed record CacheMetadata
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]

    public DateTime LastUpdated { get; set; }
}