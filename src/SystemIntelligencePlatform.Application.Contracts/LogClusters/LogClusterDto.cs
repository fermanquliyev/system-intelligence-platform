using System;
using Volo.Abp.Application.Dtos;

namespace SystemIntelligencePlatform.LogClusters;

public class LogClusterDto : FullAuditedEntityDto<Guid>
{
    public Guid? ApplicationId { get; set; }
    public string SignatureHash { get; set; } = null!;
    public string Summary { get; set; } = null!;
    public DateTime FirstSeen { get; set; }
    public DateTime LastSeen { get; set; }
    public int EventCount { get; set; }
}
