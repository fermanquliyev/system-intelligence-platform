using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SystemIntelligencePlatform.Incidents;
using SystemIntelligencePlatform.Metrics;
using SystemIntelligencePlatform.Permissions;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SystemIntelligencePlatform.Metrics;

[Authorize(SystemIntelligencePlatformPermissions.Metrics.Default)]
public class MetricAppService : ApplicationService, IMetricAppService
{
    private readonly IRepository<MetricSample, Guid> _metricRepository;
    private readonly IIncidentRepository _incidentRepository;

    public MetricAppService(
        IRepository<MetricSample, Guid> metricRepository,
        IIncidentRepository incidentRepository)
    {
        _metricRepository = metricRepository;
        _incidentRepository = incidentRepository;
    }

    [Authorize(SystemIntelligencePlatformPermissions.Metrics.Ingest)]
    public async Task IngestAsync(MetricBatchIngestDto input)
    {
        foreach (var item in input.Samples)
        {
            var e = new MetricSample(GuidGenerator.Create(), item.ApplicationId, item.Name, item.Timestamp, item.Value)
            {
                TagsJson = item.TagsJson
            };
            await _metricRepository.InsertAsync(e);
        }
    }

    public async Task<MetricSeriesDto> GetSeriesAsync(GetMetricSeriesInput input)
    {
        var q = await _metricRepository.GetQueryableAsync();
        var rows = await AsyncExecuter.ToListAsync(
            q.Where(m => m.ApplicationId == input.ApplicationId &&
                         m.Name == input.Name &&
                         m.Timestamp >= input.FromUtc &&
                         m.Timestamp <= input.ToUtc)
                .OrderBy(m => m.Timestamp)
                .Take(input.MaxPoints));

        return new MetricSeriesDto
        {
            Name = input.Name,
            Points = rows.Select(m => new MetricSeriesPointDto { Timestamp = m.Timestamp, Value = m.Value }).ToList()
        };
    }

    public async Task<IReadOnlyList<MetricSeriesDto>> GetCorrelationForIncidentAsync(Guid incidentId)
    {
        var inc = await _incidentRepository.GetAsync(incidentId);
        var from = inc.FirstOccurrence.AddHours(-2);
        var to = inc.LastOccurrence.AddHours(2);

        var q = await _metricRepository.GetQueryableAsync();
        var names = await AsyncExecuter.ToListAsync(
            q.Where(m => m.ApplicationId == inc.ApplicationId && m.Timestamp >= from && m.Timestamp <= to)
                .Select(m => m.Name)
                .Distinct()
                .Take(12));

        var list = new List<MetricSeriesDto>();
        foreach (var name in names)
        {
            var series = await GetSeriesAsync(new GetMetricSeriesInput
            {
                ApplicationId = inc.ApplicationId,
                Name = name,
                FromUtc = from,
                ToUtc = to,
                MaxPoints = 300
            });
            if (series.Points.Count > 0)
                list.Add(series);
        }

        return list;
    }
}
