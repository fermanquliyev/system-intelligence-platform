using System;
using Volo.Abp.Application.Dtos;
using SystemIntelligencePlatform.LogEvents;

namespace SystemIntelligencePlatform.LogSearch;

public class LogEventSearchItemDto : EntityDto<Guid>
{
    public Guid ApplicationId { get; set; }
    public string ApplicationName { get; set; } = null!;
    public LogLevel Level { get; set; }
    public string Message { get; set; } = null!;
    public DateTime Timestamp { get; set; }
    public bool ContainsPii { get; set; }
}
