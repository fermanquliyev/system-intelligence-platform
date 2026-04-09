using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using SystemIntelligencePlatform.Copilot;
using SystemIntelligencePlatform.Incidents;
using SystemIntelligencePlatform.LogEvents;
using SystemIntelligencePlatform.Metrics;
using SystemIntelligencePlatform.MonitoredApplications;
using SystemIntelligencePlatform.Permissions;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SystemIntelligencePlatform.Copilot;

[Authorize(SystemIntelligencePlatformPermissions.Incidents.Copilot)]
public class AiCopilotAppService : ApplicationService, IAiCopilotAppService
{
    private readonly IIncidentRepository _incidentRepository;
    private readonly IRepository<LogEvent, Guid> _logEventRepository;
    private readonly IRepository<CopilotConversationMessage, Guid> _copilotMessageRepository;
    private readonly IRepository<MetricSample, Guid> _metricRepository;
    private readonly IRepository<MonitoredApplication, Guid> _applicationRepository;
    private readonly IIncidentAiAnalyzer _analyzer;
    private readonly IMemoryCache _cache;

    public AiCopilotAppService(
        IIncidentRepository incidentRepository,
        IRepository<LogEvent, Guid> logEventRepository,
        IRepository<CopilotConversationMessage, Guid> copilotMessageRepository,
        IRepository<MetricSample, Guid> metricRepository,
        IRepository<MonitoredApplication, Guid> applicationRepository,
        IIncidentAiAnalyzer analyzer,
        IMemoryCache cache)
    {
        _incidentRepository = incidentRepository;
        _logEventRepository = logEventRepository;
        _copilotMessageRepository = copilotMessageRepository;
        _metricRepository = metricRepository;
        _applicationRepository = applicationRepository;
        _analyzer = analyzer;
        _cache = cache;
    }

    public async Task<CopilotAnalysisDto> GetAnalysisAsync(Guid incidentId)
    {
        var incident = await _incidentRepository.GetAsync(incidentId);
        var app = await _applicationRepository.FindAsync(incident.ApplicationId);
        var appName = app?.Name ?? "Unknown";

        var logLines = await GetRecentLogMessagesAsync(incident.ApplicationId, incidentId, 40);
        var templateLines = CopilotPromptTemplates.BuildIncidentAnalysisLines(incident, appName, logLines, await GetMetricSummariesAsync(incident));
        var patternHash = CopilotCacheKeyBuilder.ComputePatternHash(templateLines);
        var cacheKey = $"copilot:{CopilotPromptTemplates.Version}:{incidentId}:{patternHash}";

        if (_cache.TryGetValue(cacheKey, out CopilotAnalysisDto? cached) && cached != null)
        {
            cached.FromCache = true;
            return cached;
        }

        var analysis = await _analyzer.AnalyzeAsync(templateLines);
        var dto = MapToDto(analysis, incident);
        dto.PromptTemplateVersion = CopilotPromptTemplates.Version;
        dto.FromCache = false;

        _cache.Set(cacheKey, dto, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
        });

        return dto;
    }

    public async Task<CopilotMessageDto> PostFollowUpAsync(Guid incidentId, CopilotFollowUpInput input)
    {
        await _incidentRepository.GetAsync(incidentId);

        var userMsg = new CopilotConversationMessage(GuidGenerator.Create(), incidentId, CopilotMessageRole.User, input.Message);
        await _copilotMessageRepository.InsertAsync(userMsg);

        var history = (await _copilotMessageRepository.GetQueryableAsync())
            .Where(m => m.IncidentId == incidentId)
            .OrderBy(m => m.CreationTime)
            .Take(30)
            .ToList();

        var historyTuples = history
            .Select(m => (m.Role, m.Content))
            .ToList();

        var block = CopilotPromptTemplates.BuildFollowUpBlock(historyTuples, input.Message);
        var analysis = await _analyzer.AnalyzeAsync(new[] { block });

        var assistantText = string.Join("\n\n",
            new[]
            {
                analysis.RootCauseSummary,
                string.IsNullOrWhiteSpace(analysis.SuggestedFix) ? null : "Suggested fix: " + analysis.SuggestedFix
            }.Where(s => !string.IsNullOrWhiteSpace(s)));

        if (string.IsNullOrWhiteSpace(assistantText))
            assistantText = "No response generated.";

        var assistantMsg = new CopilotConversationMessage(
            GuidGenerator.Create(),
            incidentId,
            CopilotMessageRole.Assistant,
            assistantText);
        await _copilotMessageRepository.InsertAsync(assistantMsg);

        return ObjectMapper.Map<CopilotConversationMessage, CopilotMessageDto>(assistantMsg);
    }

    public async Task<IReadOnlyList<CopilotMessageDto>> GetConversationAsync(Guid incidentId)
    {
        await _incidentRepository.GetAsync(incidentId);
        var q = await _copilotMessageRepository.GetQueryableAsync();
        var list = q.Where(m => m.IncidentId == incidentId)
            .OrderBy(m => m.CreationTime)
            .ToList();
        return ObjectMapper.Map<List<CopilotConversationMessage>, List<CopilotMessageDto>>(list);
    }

    private async Task<List<string>> GetRecentLogMessagesAsync(Guid applicationId, Guid incidentId, int take)
    {
        var q = await _logEventRepository.GetQueryableAsync();
        var query = q.Where(e => e.ApplicationId == applicationId)
            .OrderByDescending(e => e.Timestamp)
            .Take(take);

        var logs = await AsyncExecuter.ToListAsync(query);
        return logs
            .OrderBy(e => e.Timestamp)
            .Select(e => $"[{e.Level}] {e.Timestamp:O} {e.Message}")
            .ToList();
    }

    private async Task<List<string>?> GetMetricSummariesAsync(Incident incident)
    {
        var from = incident.FirstOccurrence.AddHours(-1);
        var to = incident.LastOccurrence.AddHours(1);
        var q = await _metricRepository.GetQueryableAsync();
        var raw = await AsyncExecuter.ToListAsync(
            q.Where(m => m.ApplicationId == incident.ApplicationId && m.Timestamp >= from && m.Timestamp <= to)
                .OrderByDescending(m => m.Timestamp)
                .Take(2000));

        var rows = raw
            .GroupBy(m => m.Name)
            .Select(g => new { Name = g.Key, Avg = g.Average(x => x.Value), Max = g.Max(x => x.Value), Count = g.Count() })
            .Take(8)
            .ToList();

        if (rows.Count == 0)
            return null;

        return rows.Select(r => $"{r.Name}: avg={r.Avg:F2} max={r.Max:F2} (n={r.Count})").ToList();
    }

    private static CopilotAnalysisDto MapToDto(AiAnalysisResult analysis, Incident incident)
    {
        return new CopilotAnalysisDto
        {
            RootCauseHypothesis = analysis.RootCauseSummary ?? "",
            SuggestedFixSteps = analysis.SuggestedFix ?? "",
            ConfidenceScore = analysis.ConfidenceScore,
            SuggestedSeverity = analysis.SuggestedSeverity ?? incident.Severity,
            SeverityJustification = analysis.SeverityJustification
        };
    }
}
