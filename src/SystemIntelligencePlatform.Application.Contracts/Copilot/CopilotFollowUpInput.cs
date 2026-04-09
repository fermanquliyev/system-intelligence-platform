using System.ComponentModel.DataAnnotations;

namespace SystemIntelligencePlatform.Copilot;

public class CopilotFollowUpInput
{
    [Required]
    [StringLength(4000)]
    public string Message { get; set; } = null!;
}
