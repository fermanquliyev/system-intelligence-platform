using System;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace SystemIntelligencePlatform.Subscriptions;

public class MonthlyUsage : BasicAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Stored as YYYYMM integer for easy partitioning and querying.
    /// </summary>
    public int Month { get; private set; }

    public long LogsIngested { get; private set; }
    public int AiCallsUsed { get; private set; }

    protected MonthlyUsage() { }

    public MonthlyUsage(Guid id, int month, Guid? tenantId = null) : base(id)
    {
        TenantId = tenantId;
        Month = month;
    }

    public void IncrementLogs(int count)
    {
        LogsIngested += count;
    }

    public void IncrementAiCalls(int count = 1)
    {
        AiCallsUsed += count;
    }

    public static int CurrentMonth() =>
        DateTime.UtcNow.Year * 100 + DateTime.UtcNow.Month;
}
