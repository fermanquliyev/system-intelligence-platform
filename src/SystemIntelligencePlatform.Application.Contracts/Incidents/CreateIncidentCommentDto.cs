using System.ComponentModel.DataAnnotations;

namespace SystemIntelligencePlatform.Incidents;

public class CreateIncidentCommentDto
{
    [Required]
    [StringLength(2000)]
    public string Content { get; set; } = null!;
}
