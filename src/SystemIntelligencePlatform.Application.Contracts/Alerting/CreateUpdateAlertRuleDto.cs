using System;
using System.ComponentModel.DataAnnotations;

namespace SystemIntelligencePlatform.Alerting;

public class CreateUpdateAlertRuleDto
{
    [Required]
    [StringLength(256)]
    public string Name { get; set; } = null!;

    public bool IsEnabled { get; set; } = true;

    [Required]
    public string DefinitionJson { get; set; } = "{}";

    public int? SeverityOverride { get; set; }

    public Guid? ApplicationId { get; set; }
}
