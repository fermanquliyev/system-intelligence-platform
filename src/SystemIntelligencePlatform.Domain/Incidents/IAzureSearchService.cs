using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SystemIntelligencePlatform.Incidents;

/// <summary>
/// Indexes and searches incidents via a search index.
/// Infrastructure layer provides the Azure AI Search implementation.
/// </summary>
public interface IAzureSearchService
{
    Task IndexIncidentAsync(IncidentSearchDocument document);
    Task<IncidentSearchResult> SearchAsync(string query, Guid? tenantId, int skip = 0, int take = 20);
    Task DeleteDocumentAsync(Guid incidentId);
}

public class IncidentSearchDocument
{
    public string Id { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string Severity { get; set; } = null!;
    public string ApplicationName { get; set; } = null!;
    public string? KeyPhrases { get; set; }
    public string? Entities { get; set; }
    public string? TenantId { get; set; }
}

public class IncidentSearchResult
{
    public long TotalCount { get; set; }
    public List<IncidentSearchDocument> Documents { get; set; } = new();
}
