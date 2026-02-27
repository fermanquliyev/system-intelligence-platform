using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SystemIntelligencePlatform.LogEvents;

namespace SystemIntelligencePlatform.LogIngestion;

public class LogIngestionDto
{
    [Required]
    public List<LogIngestionItemDto> Events { get; set; } = new();
}

public class LogIngestionItemDto
{
    [Required]
    public LogLevel Level { get; set; }

    [Required]
    [StringLength(LogEventConsts.MaxMessageLength)]
    public string Message { get; set; } = null!;

    [StringLength(LogEventConsts.MaxSourceLength)]
    public string? Source { get; set; }

    [StringLength(LogEventConsts.MaxExceptionTypeLength)]
    public string? ExceptionType { get; set; }

    [StringLength(LogEventConsts.MaxStackTraceLength)]
    public string? StackTrace { get; set; }

    [StringLength(LogEventConsts.MaxCorrelationIdLength)]
    public string? CorrelationId { get; set; }

    public DateTime? Timestamp { get; set; }
}
