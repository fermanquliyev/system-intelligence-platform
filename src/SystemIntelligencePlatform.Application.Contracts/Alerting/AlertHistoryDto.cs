using System;
using Volo.Abp.Application.Dtos;

namespace SystemIntelligencePlatform.Alerting;

public class AlertHistoryDto : CreationAuditedEntityDto<Guid>
{
    public Guid AlertRuleId { get; set; }
    public Guid? ApplicationId { get; set; }
    public string PayloadJson { get; set; } = null!;
    public DateTime FiredAt { get; set; }
}
