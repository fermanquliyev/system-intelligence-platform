using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace SystemIntelligencePlatform.Alerting;

public class AlertHistory : CreationAuditedEntity<Guid>
{
    public Guid AlertRuleId { get; set; }
    public Guid? ApplicationId { get; set; }
    public string PayloadJson { get; set; } = "{}";
    public DateTime FiredAt { get; set; }

    protected AlertHistory() { }

    public AlertHistory(Guid id, Guid alertRuleId, DateTime firedAt, string payloadJson, Guid? applicationId = null)
        : base(id)
    {
        AlertRuleId = alertRuleId;
        FiredAt = firedAt;
        PayloadJson = payloadJson;
        ApplicationId = applicationId;
    }
}
