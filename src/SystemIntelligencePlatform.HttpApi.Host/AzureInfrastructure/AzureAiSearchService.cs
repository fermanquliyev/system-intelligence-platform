using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Logging;
using SystemIntelligencePlatform.Incidents;
using Volo.Abp.DependencyInjection;

namespace SystemIntelligencePlatform.AzureInfrastructure;

public class AzureAiSearchService : IAzureSearchService, ITransientDependency
{
    private const string IndexName = "incidents-index";

    private readonly SearchClient _searchClient;
    private readonly SearchIndexClient _indexClient;
    private readonly ILogger<AzureAiSearchService> _logger;

    public AzureAiSearchService(
        SearchClient searchClient,
        SearchIndexClient indexClient,
        ILogger<AzureAiSearchService> logger)
    {
        _searchClient = searchClient;
        _indexClient = indexClient;
        _logger = logger;
    }

    public async Task EnsureIndexExistsAsync()
    {
        var definition = new SearchIndex(IndexName)
        {
            Fields = new List<SearchField>
            {
                new SimpleField("Id", SearchFieldDataType.String) { IsKey = true, IsFilterable = true },
                new SearchableField("Title") { AnalyzerName = LexicalAnalyzerName.EnMicrosoft },
                new SearchableField("Description") { AnalyzerName = LexicalAnalyzerName.EnMicrosoft },
                new SimpleField("Severity", SearchFieldDataType.String) { IsFilterable = true, IsFacetable = true },
                new SimpleField("ApplicationName", SearchFieldDataType.String) { IsFilterable = true, IsFacetable = true },
                new SearchableField("KeyPhrases"),
                new SearchableField("Entities"),
                new SimpleField("TenantId", SearchFieldDataType.String) { IsFilterable = true }
            }
        };

        try
        {
            await _indexClient.CreateOrUpdateIndexAsync(definition);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create/update search index {IndexName}", IndexName);
        }
    }

    public async Task IndexIncidentAsync(IncidentSearchDocument document)
    {
        try
        {
            var batch = IndexDocumentsBatch.Upload(new[] { document });
            await _searchClient.IndexDocumentsAsync(batch);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to index incident {IncidentId}", document.Id);
        }
    }

    public async Task<IncidentSearchResult> SearchAsync(string query, Guid? tenantId, int skip = 0, int take = 20)
    {
        var options = new SearchOptions
        {
            Skip = skip,
            Size = take,
            IncludeTotalCount = true
        };

        if (tenantId.HasValue)
        {
            options.Filter = $"TenantId eq '{tenantId.Value}'";
        }

        options.Select.Add("Id");
        options.Select.Add("Title");
        options.Select.Add("Description");
        options.Select.Add("Severity");
        options.Select.Add("ApplicationName");
        options.Select.Add("KeyPhrases");
        options.Select.Add("Entities");

        try
        {
            var response = await _searchClient.SearchAsync<IncidentSearchDocument>(query, options);
            var documents = new List<IncidentSearchDocument>();

            await foreach (var result in response.Value.GetResultsAsync())
            {
                documents.Add(result.Document);
            }

            return new IncidentSearchResult
            {
                TotalCount = response.Value.TotalCount ?? 0,
                Documents = documents
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Search query failed: {Query}", query);
            return new IncidentSearchResult();
        }
    }

    public async Task DeleteDocumentAsync(Guid incidentId)
    {
        try
        {
            var batch = IndexDocumentsBatch.Delete("Id", new[] { incidentId.ToString() });
            await _searchClient.IndexDocumentsAsync(batch);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete incident {IncidentId} from search index", incidentId);
        }
    }
}
