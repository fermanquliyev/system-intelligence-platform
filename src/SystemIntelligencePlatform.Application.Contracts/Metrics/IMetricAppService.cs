using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SystemIntelligencePlatform.Metrics;

public interface IMetricAppService : IApplicationService
{
    Task IngestAsync(MetricBatchIngestDto input);

    Task<MetricSeriesDto> GetSeriesAsync(GetMetricSeriesInput input);

    Task<IReadOnlyList<MetricSeriesDto>> GetCorrelationForIncidentAsync(Guid incidentId);
}
