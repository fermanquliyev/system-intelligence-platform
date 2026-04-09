using System;
using Volo.Abp.Application.Dtos;
using SystemIntelligencePlatform.LogEvents;

namespace SystemIntelligencePlatform.LogSearch;

public class LogSearchInput : PagedAndSortedResultRequestDto
{
    public string? Query { get; set; }

    public bool UseFullText { get; set; } = true;

    public Guid? ApplicationId { get; set; }

    public LogLevel? MinLevel { get; set; }

    public DateTime? FromUtc { get; set; }

    public DateTime? ToUtc { get; set; }

    /// <summary>When true and user has ViewUnmasked, show raw log text.</summary>
    public bool RevealSensitive { get; set; }
}
