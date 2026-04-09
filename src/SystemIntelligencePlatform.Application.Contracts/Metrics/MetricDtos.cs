using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace SystemIntelligencePlatform.Metrics;

public class MetricSampleIngestDto
{
    [Required]
    public Guid ApplicationId { get; set; }

    [Required]
    [StringLength(128)]
    public string Name { get; set; } = null!;

    public DateTime Timestamp { get; set; }

    public double Value { get; set; }

    [StringLength(2000)]
    public string? TagsJson { get; set; }
}

public class MetricBatchIngestDto
{
    [Required]
    [MinLength(1)]
    public List<MetricSampleIngestDto> Samples { get; set; } = new();
}

public class GetMetricSeriesInput
{
    public Guid ApplicationId { get; set; }
    public string Name { get; set; } = null!;
    public DateTime FromUtc { get; set; }
    public DateTime ToUtc { get; set; }
    public int MaxPoints { get; set; } = 500;
}

public class MetricSeriesPointDto
{
    public DateTime Timestamp { get; set; }
    public double Value { get; set; }
}

public class MetricSeriesDto
{
    [Required]
    [StringLength(128)]
    public string Name { get; set; } = null!;

    public List<MetricSeriesPointDto> Points { get; set; } = new();
}

public class IncidentMetricCorrelationDto
{
    public Guid IncidentId { get; set; }
    public List<MetricSeriesPointDto> Series { get; set; } = new();
    public List<IncidentCorrelationMarkerDto> Markers { get; set; } = new();
}

public class IncidentCorrelationMarkerDto
{
    public DateTime Timestamp { get; set; }
    public string Label { get; set; } = null!;
}
