using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SystemIntelligencePlatform.MonitoredApplications;
using SystemIntelligencePlatform.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SystemIntelligencePlatform.Incidents;

[Authorize(SystemIntelligencePlatformPermissions.Incidents.Default)]
public class IncidentAppService : ApplicationService, IIncidentAppService
{
    private readonly IIncidentRepository _incidentRepository;
    private readonly IRepository<IncidentComment, Guid> _commentRepository;
    private readonly IRepository<MonitoredApplication, Guid> _applicationRepository;
    private readonly IIncidentSearchService _searchService;

    public IncidentAppService(
        IIncidentRepository incidentRepository,
        IRepository<IncidentComment, Guid> commentRepository,
        IRepository<MonitoredApplication, Guid> applicationRepository,
        IIncidentSearchService searchService)
    {
        _incidentRepository = incidentRepository;
        _commentRepository = commentRepository;
        _applicationRepository = applicationRepository;
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

        return dto;
    }

    public async Task<PagedResultDto<IncidentDto>> GetListAsync(GetIncidentListInput input)
    {
        var queryable = await _incidentRepository.GetQueryableAsync();

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

        var dtos = incidents.Select(i => MapIncidentListDto(i, appLookup.GetValueOrDefault(i.ApplicationId, "Unknown"))).ToList();

        return new PagedResultDto<IncidentDto>(totalCount, dtos);
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
