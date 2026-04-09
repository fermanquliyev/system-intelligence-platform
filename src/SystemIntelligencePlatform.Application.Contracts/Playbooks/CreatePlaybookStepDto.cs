using System.ComponentModel.DataAnnotations;

namespace SystemIntelligencePlatform.Playbooks;

public class CreatePlaybookStepDto
{
    public int SortOrder { get; set; }

    [Required]
    [StringLength(512)]
    public string Title { get; set; } = null!;

    [StringLength(4000)]
    public string? Body { get; set; }
}
