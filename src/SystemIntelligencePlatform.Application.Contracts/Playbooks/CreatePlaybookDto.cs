using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SystemIntelligencePlatform.Playbooks;

public class CreatePlaybookDto
{
    [Required]
    [StringLength(256)]
    public string Name { get; set; } = null!;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required]
    public string TriggerDefinitionJson { get; set; } = "{}";

    public List<CreatePlaybookStepDto> Steps { get; set; } = new();
}
