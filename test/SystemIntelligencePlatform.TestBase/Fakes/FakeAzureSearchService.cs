using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SystemIntelligencePlatform.Incidents;
using Volo.Abp.DependencyInjection;

namespace SystemIntelligencePlatform.Fakes;

[Dependency(ReplaceServices = true)]
public class FakeSearchService : IIncidentSearchService, ITransientDependency
{
    public Task IndexIncidentAsync(IncidentSearchDocument document)
    {
        return Task.CompletedTask;
    }

    public Task<IncidentSearchResult> SearchAsync(string query, int skip = 0, int take = 20)
    {
        return Task.FromResult(new IncidentSearchResult
        {
            Documents = new List<IncidentSearchDocument>(),
            TotalCount = 0
        });
    }

    public Task DeleteDocumentAsync(Guid incidentId)
    {
        return Task.CompletedTask;
    }
}
