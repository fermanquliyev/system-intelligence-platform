using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SystemIntelligencePlatform.LogEvents;
using SystemIntelligencePlatform.Metrics;
using SystemIntelligencePlatform.MonitoredApplications;
using SystemIntelligencePlatform.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace SystemIntelligencePlatform.Incidents;

[Authorize(SystemIntelligencePlatformPermissions.Incidents.Default)]
public class IncidentAppService : ApplicationService, IIncidentAppService
{
    private readonly IIncidentRepository _incidentRepository;
    private readonly IRepository<IncidentComment, Guid> _commentRepository;
    private readonly IRepository<MonitoredApplication, Guid> _applicationRepository;
    private readonly IRepository<MergedIncidentLink, Guid> _mergedLinkRepository;
    private readonly IRepository<LogEvent, Guid> _logEventRepository;
    private readonly IRepository<MetricSample, Guid> _metricRepository;
    private readonly IIdentityUserRepository _userRepository;
    private readonly IIncidentSearchService _searchService;

    public IncidentAppService(
        IIncidentRepository incidentRepository,
        IRepository<IncidentComment, Guid> commentRepository,
        IRepository<MonitoredApplication, Guid> applicationRepository,
        IRepository<MergedIncidentLink, Guid> mergedLinkRepository,
        IRepository<LogEvent, Guid> logEventRepository,
        IRepository<MetricSample, Guid> metricRepository,
        IIdentityUserRepository userRepository,
        IIncidentSearchService searchService)
    {
        _incidentRepository = incidentRepository;
        _commentRepository = commentRepository;
        _applicationRepository = applicationRepository;
        _mergedLinkRepository = mergedLinkRepository;
        _logEventRepository = logEventRepository;
        _metricRepository = metricRepository;
        _userRepository = userRepository;
        _searchService = searchService;
    }

    public async Task<IncidentDto> GetAsync(Guid id)
    {
        var incident = await _incidentRepository.GetAsync(id);
        var dto = ObjectMapper.Map<Incident, IncidentDto>(incident);
        var app = await _applicationRepository.FindAsync(incident.ApplicationId);
        dto.ApplicationName = app?.Name ?? "Unknown";

        var comments = await GetCommentsInternal(id);
        dto.Comments = ObjectMapper.Map<List<IncidentComment>, List<IncidentCommentDto>>(comments);

        if (incident.AssignedUserId.HasValue)
        {
            var user = await _userRepository.FindAsync(incident.AssignedUserId.Value);
            dto.AssigneeUserName = user?.UserName;
        }

        var mergeQ = await _mergedLinkRepository.GetQueryableAsync();
        dto.MergedChildIncidentIds = await AsyncExecuter.ToListAsync(
            mergeQ.Where(l => l.CanonicalIncidentId == id).Select(l => l.MergedIncidentId));

        return dto;
    }

    public async Task<PagedResultDto<IncidentDto>> GetListAsync(GetIncidentListInput input)
    {
        var queryable = await _incidentRepository.GetQueryableAsync();
        queryable = queryable.Where(i => i.MergedIntoIncidentId == null);

        if (input.ApplicationId.HasValue)
            queryable = queryable.Where(i => i.ApplicationId == input.ApplicationId.Value);

        if (input.Severity.HasValue)
            queryable = queryable.Where(i => i.Severity == input.Severity.Value);

        if (input.Status.HasValue)
            queryable = queryable.Where(i => i.Status == input.Status.Value);

        if (!input.Filter.IsNullOrWhiteSpace())
            queryable = queryable.Where(i => i.Title.Contains(input.Filter!));

        var totalCount = await AsyncExecuter.CountAsync(queryable);

        var sorted = ApplyIncidentSorting(queryable, input.Sorting)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount);

        var incidents = await AsyncExecuter.ToListAsync(sorted);
        var appIds = incidents.Select(i => i.ApplicationId).Distinct().ToList();
        var appQueryable = await _applicationRepository.GetQueryableAsync();
        var apps = await AsyncExecuter.ToListAsync(appQueryable.Where(a => appIds.Contains(a.Id)));
        var appLookup = apps.ToDictionary(a => a.Id, a => a.Name);

        var assigneeIds = incidents.Where(i => i.AssignedUserId.HasValue).Select(i => i.AssignedUserId!.Value).Distinct().ToList();
        var userLookup = new Dictionary<Guid, string?>();
        foreach (var aid in assigneeIds)
        {
            var u = await _userRepository.FindAsync(aid);
            if (u != null && u.UserName != null)
                userLookup[aid] = u.UserName;
        }

        var dtos = incidents.Select(i =>
        {
            var d = MapIncidentListDto(i, appLookup.GetValueOrDefault(i.ApplicationId, "Unknown"));
            if (i.AssignedUserId.HasValue && userLookup.TryGetValue(i.AssignedUserId.Value, out var un))
                d.AssigneeUserName = un;
            return d;
        }).ToList();

        return new PagedResultDto<IncidentDto>(totalCount, dtos);
    }

    [Authorize(SystemIntelligencePlatformPermissions.Incidents.Assign)]
    public async Task<IncidentDto> AssignAsync(Guid id, AssignIncidentInput input)
    {
        var incident = await _incidentRepository.GetAsync(id);
        var userId = input.UserId;
        if (userId.HasValue)
            await _userRepository.GetAsync(userId.Value);

        incident.AssignTo(userId);
        await _incidentRepository.UpdateAsync(incident);

        return await GetAsync(id);
    }

    public async Task<IReadOnlyList<IncidentDto>> GetMergedChildrenAsync(Guid canonicalIncidentId)
    {
        await _incidentRepository.GetAsync(canonicalIncidentId);
        var q = await _mergedLinkRepository.GetQueryableAsync();
        var ids = await AsyncExecuter.ToListAsync(q.Where(l => l.CanonicalIncidentId == canonicalIncidentId).Select(l => l.MergedIncidentId));
        var list = new List<IncidentDto>();
        foreach (var mid in ids)
        {
            var inc = await _incidentRepository.FindAsync(mid);
            if (inc == null) continue;
            var dto = ObjectMapper.Map<Incident, IncidentDto>(inc);
            var app = await _applicationRepository.FindAsync(inc.ApplicationId);
            dto.ApplicationName = app?.Name ?? "Unknown";
            list.Add(dto);
        }

        return list;
    }

    [Authorize(SystemIntelligencePlatformPermissions.Incidents.Timeline)]
    public async Task<PagedResultDto<IncidentTimelineItemDto>> GetGlobalTimelineAsync(GetGlobalTimelineInput input)
    {
        var queryable = await _incidentRepository.GetQueryableAsync();
        queryable = queryable.Where(i => i.MergedIntoIncidentId == null);

        if (input.ApplicationId.HasValue)
            queryable = queryable.Where(i => i.ApplicationId == input.ApplicationId.Value);

        if (input.Severity.HasValue)
            queryable = queryable.Where(i => i.Severity == input.Severity.Value);

        if (input.FromUtc.HasValue)
            queryable = queryable.Where(i => i.LastOccurrence >= input.FromUtc.Value);

        if (input.ToUtc.HasValue)
            queryable = queryable.Where(i => i.LastOccurrence <= input.ToUtc.Value);

        var totalCount = await AsyncExecuter.CountAsync(queryable);
        var sorted = queryable.OrderByDescending(i => i.LastOccurrence)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount);

        var incidents = await AsyncExecuter.ToListAsync(sorted);
        var appIds = incidents.Select(i => i.ApplicationId).Distinct().ToList();
        var appQueryable = await _applicationRepository.GetQueryableAsync();
        var apps = await AsyncExecuter.ToListAsync(appQueryable.Where(a => appIds.Contains(a.Id)));
        var appLookup = apps.ToDictionary(a => a.Id, a => a.Name);

        var items = incidents.Select(i => new IncidentTimelineItemDto
        {
            Id = i.Id,
            Timestamp = i.LastOccurrence,
            Title = i.Title,
            Severity = i.Severity,
            Status = i.Status,
            ApplicationName = appLookup.GetValueOrDefault(i.ApplicationId, "Unknown"),
            Kind = "Incident"
        }).ToList();

        return new PagedResultDto<IncidentTimelineItemDto>(totalCount, items);
    }

    [Authorize(SystemIntelligencePlatformPermissions.Incidents.Timeline)]
    public async Task<IReadOnlyList<IncidentRootCauseTimelineItemDto>> GetRootCauseTimelineAsync(Guid incidentId)
    {
        var inc = await _incidentRepository.GetAsync(incidentId);
        var from = inc.FirstOccurrence.AddHours(-2);
        var to = inc.LastOccurrence.AddHours(2);

        var items = new List<IncidentRootCauseTimelineItemDto>
        {
            new()
            {
                EventType = "IncidentOpened",
                TimestampUtc = inc.FirstOccurrence,
                Title = "Incident first seen",
                Detail = inc.Title,
                IsCritical = inc.Severity == IncidentSeverity.Critical
            }
        };

        if (inc.LastOccurrence != inc.FirstOccurrence)
        {
            items.Add(new IncidentRootCauseTimelineItemDto
            {
                EventType = "IncidentActivity",
                TimestampUtc = inc.LastOccurrence,
                Title = "Last occurrence",
                Detail = $"Count: {inc.OccurrenceCount}",
                IsCritical = false
            });
        }

        var logQ = await _logEventRepository.GetQueryableAsync();
        var logs = await AsyncExecuter.ToListAsync(
            logQ.Where(e =>
                    e.ApplicationId == inc.ApplicationId &&
                    e.Timestamp >= from &&
                    e.Timestamp <= to &&
                    (e.IncidentId == incidentId || e.Level >= LogLevel.Error))
                .OrderBy(e => e.Timestamp)
                .Take(500));

        foreach (var e in logs)
        {
            var critical = e.Level == LogLevel.Critical;
            items.Add(new IncidentRootCauseTimelineItemDto
            {
                EventType = "Log",
                TimestampUtc = e.Timestamp,
                Title = e.Level.ToString(),
                Detail = e.Message.Length > 500 ? e.Message[..500] + "..." : e.Message,
                IsCritical = critical
            });
        }

        var metQ = await _metricRepository.GetQueryableAsync();
        var metrics = await AsyncExecuter.ToListAsync(
            metQ.Where(m => m.ApplicationId == inc.ApplicationId && m.Timestamp >= from && m.Timestamp <= to)
                .OrderBy(m => m.Timestamp)
                .Take(300));

        foreach (var m in metrics)
        {
            items.Add(new IncidentRootCauseTimelineItemDto
            {
                EventType = "Metric",
                TimestampUtc = m.Timestamp,
                Title = m.Name,
                Detail = m.Value.ToString("F2"),
                IsCritical = false
            });
        }

        return items.OrderBy(i => i.TimestampUtc).ToList();
    }

    [Authorize(SystemIntelligencePlatformPermissions.Incidents.Resolve)]
    public async Task<IncidentDto> ResolveAsync(Guid id)
    {
        var incident = await _incidentRepository.GetAsync(id);
        incident.Resolve(CurrentUser.Id!.Value);
        await _incidentRepository.UpdateAsync(incident);

        var dto = ObjectMapper.Map<Incident, IncidentDto>(incident);
        var app = await _applicationRepository.FindAsync(incident.ApplicationId);
        dto.ApplicationName = app?.Name ?? "Unknown";
        return dto;
    }

    [Authorize(SystemIntelligencePlatformPermissions.Incidents.Update)]
    public async Task<IncidentDto> UpdateStatusAsync(Guid id, IncidentStatus status)
    {
        var incident = await _incidentRepository.GetAsync(id);
        incident.Status = status;
        await _incidentRepository.UpdateAsync(incident);

        var dto = ObjectMapper.Map<Incident, IncidentDto>(incident);
        var app = await _applicationRepository.FindAsync(incident.ApplicationId);
        dto.ApplicationName = app?.Name ?? "Unknown";
        return dto;
    }

    [Authorize(SystemIntelligencePlatformPermissions.Incidents.Comment)]
    public async Task<IncidentCommentDto> AddCommentAsync(Guid incidentId, CreateIncidentCommentDto input)
    {
        await _incidentRepository.GetAsync(incidentId);

        var comment = new IncidentComment(
            GuidGenerator.Create(),
            incidentId,
            input.Content
        );

        await _commentRepository.InsertAsync(comment);
        return ObjectMapper.Map<IncidentComment, IncidentCommentDto>(comment);
    }

    public async Task<PagedResultDto<IncidentCommentDto>> GetCommentsAsync(Guid incidentId, PagedResultRequestDto input)
    {
        var queryable = await _commentRepository.GetQueryableAsync();
        var query = queryable
            .Where(c => c.IncidentId == incidentId)
            .OrderByDescending(c => c.CreationTime);

        var totalCount = await AsyncExecuter.CountAsync(query);
        var comments = await AsyncExecuter.ToListAsync(query.Skip(input.SkipCount).Take(input.MaxResultCount));

        return new PagedResultDto<IncidentCommentDto>(
            totalCount,
            ObjectMapper.Map<List<IncidentComment>, List<IncidentCommentDto>>(comments)
        );
    }

    [Authorize(SystemIntelligencePlatformPermissions.Incidents.Search)]
    public async Task<IncidentSearchResultDto> SearchAsync(IncidentSearchRequestDto input)
    {
        var result = await _searchService.SearchAsync(input.Query, input.Skip, input.Take);

        return new IncidentSearchResultDto
        {
            TotalCount = result.TotalCount,
            Items = result.Documents.Select(d => new IncidentSearchItemDto
            {
                Id = d.Id,
                Title = d.Title,
                Description = d.Description,
                Severity = d.Severity,
                ApplicationName = d.ApplicationName,
                KeyPhrases = d.KeyPhrases,
                Entities = d.Entities
            }).ToList()
        };
    }

    private async Task<List<IncidentComment>> GetCommentsInternal(Guid incidentId)
    {
        var queryable = await _commentRepository.GetQueryableAsync();
        return await AsyncExecuter.ToListAsync(
            queryable.Where(c => c.IncidentId == incidentId).OrderByDescending(c => c.CreationTime).Take(50));
    }

    private static IQueryable<Incident> ApplyIncidentSorting(IQueryable<Incident> query, string? sorting)
    {
        if (sorting.IsNullOrWhiteSpace())
        {
            return query.OrderByDescending(i => i.LastOccurrence);
        }

        var parts = sorting.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var field = parts[0].Trim().ToLowerInvariant();
        var desc = parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

        return field switch
        {
            "title" => desc ? query.OrderByDescending(i => i.Title) : query.OrderBy(i => i.Title),
            "severity" => desc ? query.OrderByDescending(i => i.Severity) : query.OrderBy(i => i.Severity),
            "status" => desc ? query.OrderByDescending(i => i.Status) : query.OrderBy(i => i.Status),
            "occurrencecount" => desc ? query.OrderByDescending(i => i.OccurrenceCount) : query.OrderBy(i => i.OccurrenceCount),
            "firstoccurrence" => desc ? query.OrderByDescending(i => i.FirstOccurrence) : query.OrderBy(i => i.FirstOccurrence),
            "creationtime" => desc ? query.OrderByDescending(i => i.CreationTime) : query.OrderBy(i => i.CreationTime),
            "lastoccurrence" => desc ? query.OrderByDescending(i => i.LastOccurrence) : query.OrderBy(i => i.LastOccurrence),
            _ => query.OrderByDescending(i => i.LastOccurrence),
        };
    }

    private static IncidentDto MapIncidentListDto(Incident incident, string applicationName)
    {
        return new IncidentDto
        {
            Id = incident.Id,
            ApplicationId = incident.ApplicationId,
            ApplicationName = applicationName,
            Title = incident.Title,
            Description = incident.Description,
            Severity = incident.Severity,
            Status = incident.Status,
            HashSignature = incident.HashSignature,
            OccurrenceCount = incident.OccurrenceCount,
            FirstOccurrence = incident.FirstOccurrence,
            LastOccurrence = incident.LastOccurrence,
            SentimentScore = incident.SentimentScore,
            KeyPhrases = incident.KeyPhrases,
            Entities = incident.Entities,
            RootCauseSummary = incident.RootCauseSummary,
            SuggestedFix = incident.SuggestedFix,
            SeverityJustification = incident.SeverityJustification,
            ConfidenceScore = incident.ConfidenceScore,
            AiAnalyzedAt = incident.AiAnalyzedAt,
            ResolvedAt = incident.ResolvedAt,
            AssignedUserId = incident.AssignedUserId,
            MergedIntoIncidentId = incident.MergedIntoIncidentId,
            ContainsPii = incident.ContainsPii,
            CreationTime = incident.CreationTime,
            CreatorId = incident.CreatorId,
            LastModificationTime = incident.LastModificationTime,
            LastModifierId = incident.LastModifierId,
            IsDeleted = incident.IsDeleted,
            DeleterId = incident.DeleterId,
            DeletionTime = incident.DeletionTime,
            Comments = new List<IncidentCommentDto>()
        };
    }
}
