using System.ComponentModel.DataAnnotations;

namespace SystemIntelligencePlatform.LogSearch;

public class CreateSavedLogSearchDto
{
    [Required]
    [StringLength(128)]
    public string Name { get; set; } = null!;

    [Required]
    public string FilterJson { get; set; } = "{}";
}
