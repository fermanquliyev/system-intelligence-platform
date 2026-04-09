using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SystemIntelligencePlatform.Incidents;
using SystemIntelligencePlatform.MonitoredApplications;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;

namespace SystemIntelligencePlatform.EntityFrameworkCore.Incidents;

/// <summary>
/// Search incidents using EF Core queries against PostgreSQL (ILIKE for case-insensitive match).
/// </summary>
public class DatabaseIncidentSearchService : IIncidentSearchService, ITransientDependency
{
    private readonly IDbContextProvider<SystemIntelligencePlatformDbContext> _dbContextProvider;

    public DatabaseIncidentSearchService(
        IDbContextProvider<SystemIntelligencePlatformDbContext> dbContextProvider)
    {
        _dbContextProvider = dbContextProvider;
    }

    public Task IndexIncidentAsync(IncidentSearchDocument document)
    {
        return Task.CompletedTask;
    }

    public async Task<IncidentSearchResult> SearchAsync(string query, int skip = 0, int take = 20)
    {
        var db = await _dbContextProvider.GetDbContextAsync();
        var q = db.Incidents.AsNoTracking();

        var searchTerm = query?.Trim();
        if (!string.IsNullOrEmpty(searchTerm))
        {
            var pattern = $"%{searchTerm}%";
            q = q.Where(i =>
                EF.Functions.ILike(i.Title, pattern) ||
                (i.Description != null && EF.Functions.ILike(i.Description, pattern)) ||
                (i.RootCauseSummary != null && EF.Functions.ILike(i.RootCauseSummary, pattern)));
        }

        var totalCount = await q.LongCountAsync();

        var incidents = await q
            .OrderByDescending(i => i.LastOccurrence)
            .Skip(skip)
            .Take(take)
            .Select(i => new { i.Id, i.Title, i.Description, i.RootCauseSummary, i.Severity, i.ApplicationId, i.KeyPhrases, i.Entities })
            .ToListAsync();

        var appIds = incidents.Select(x => x.ApplicationId).Distinct().ToList();
        var appNames = await db.MonitoredApplications
            .AsNoTracking()
            .Where(a => appIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, a => a.Name);

        var documents = incidents.Select(i => new IncidentSearchDocument
        {
            Id = i.Id.ToString(),
            Title = i.Title,
            Description = i.Description,
            RootCauseSummary = i.RootCauseSummary,
            Severity = i.Severity.ToString(),
            ApplicationName = appNames.GetValueOrDefault(i.ApplicationId, "Unknown"),
            KeyPhrases = i.KeyPhrases,
            Entities = i.Entities
        }).ToList();

        return new IncidentSearchResult
        {
            TotalCount = totalCount,
            Documents = documents
        };
    }

    public Task DeleteDocumentAsync(System.Guid incidentId)
    {
        return Task.CompletedTask;
    }
}
