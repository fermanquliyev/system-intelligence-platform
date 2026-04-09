using System;
using Volo.Abp.Application.Dtos;

namespace SystemIntelligencePlatform.Alerting;

public class AlertRuleDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = null!;
    public bool IsEnabled { get; set; }
    public string DefinitionJson { get; set; } = null!;
    public int? SeverityOverride { get; set; }
    public Guid? ApplicationId { get; set; }
}
