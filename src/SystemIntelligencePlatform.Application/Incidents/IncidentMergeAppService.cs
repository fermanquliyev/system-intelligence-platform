using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SystemIntelligencePlatform.Permissions;
using SystemIntelligencePlatform.Text;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SystemIntelligencePlatform.Incidents;

[Authorize(SystemIntelligencePlatformPermissions.Incidents.Update)]
public class IncidentMergeAppService : ApplicationService, IIncidentMergeAppService
{
    private readonly IIncidentRepository _incidentRepository;
    private readonly IRepository<MergedIncidentLink, Guid> _mergeRepository;

    public IncidentMergeAppService(
        IIncidentRepository incidentRepository,
        IRepository<MergedIncidentLink, Guid> mergeRepository)
    {
        _incidentRepository = incidentRepository;
        _mergeRepository = mergeRepository;
    }

    public async Task<int> ScanAndMergeAsync(Guid? applicationId)
    {
        var q = await _incidentRepository.GetQueryableAsync();
        q = q.Where(i => i.MergedIntoIncidentId == null && i.Status != IncidentStatus.Resolved && i.Status != IncidentStatus.Closed);
        if (applicationId.HasValue)
            q = q.Where(i => i.ApplicationId == applicationId.Value);

        var list = await AsyncExecuter.ToListAsync(q.OrderBy(i => i.FirstOccurrence).Take(150));
        var merged = 0;

        for (var i = 0; i < list.Count; i++)
        {
            var a = list[i];
            if (a.MergedIntoIncidentId != null)
                continue;

            for (var j = i + 1; j < list.Count; j++)
            {
                var b = list[j];
                if (b.MergedIntoIncidentId != null)
                    continue;

                if (a.ApplicationId != b.ApplicationId)
                    continue;

                var dist = StringSimilarity.Levenshtein(a.Title, b.Title);
                if (dist > 3)
                    continue;

                var canonical = a.FirstOccurrence <= b.FirstOccurrence ? a : b;
                var duplicate = canonical.Id == a.Id ? b : a;

                var linkQ = await _mergeRepository.GetQueryableAsync();
                if (await AsyncExecuter.AnyAsync(linkQ.Where(l => l.MergedIncidentId == duplicate.Id)))
                    continue;

                duplicate.MarkMergedInto(canonical.Id);
                canonical.IncrementOccurrence(duplicate.LastOccurrence);
                await _mergeRepository.InsertAsync(new MergedIncidentLink(
                    GuidGenerator.Create(),
                    canonical.Id,
                    duplicate.Id,
                    1.0 - dist / 10.0));

                await _incidentRepository.UpdateAsync(duplicate);
                await _incidentRepository.UpdateAsync(canonical);
                merged++;
                break;
            }
        }

        return merged;
    }
}
