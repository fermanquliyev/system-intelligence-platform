using System;
using System.Threading.Tasks;
using SystemIntelligencePlatform.EntityFrameworkCore;
using SystemIntelligencePlatform.EntityFrameworkCore.Incidents;
using SystemIntelligencePlatform.Incidents;
using Shouldly;
using Xunit;

namespace SystemIntelligencePlatform.Infrastructure.Tests;

public class DatabaseIncidentSearchService_Tests : SystemIntelligencePlatformEntityFrameworkCoreTestBase
{
    private readonly DatabaseIncidentSearchService _searchService;

    public DatabaseIncidentSearchService_Tests()
    {
        _searchService = GetRequiredService<DatabaseIncidentSearchService>();
    }

    [Fact]
    public async Task SearchAsync_ReturnsResultStructure()
    {
        var result = await WithUnitOfWorkAsync(() => _searchService.SearchAsync("", 0, 10));
        result.ShouldNotBeNull();
        result.Documents.ShouldNotBeNull();
        result.TotalCount.ShouldBeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task IndexIncidentAsync_DoesNotThrow()
    {
        await _searchService.IndexIncidentAsync(new IncidentSearchDocument
        {
            Id = Guid.NewGuid().ToString(),
            Title = "Test",
            Description = "Desc",
            Severity = "Medium",
            ApplicationName = "App"
        });
    }

    [Fact]
    public async Task DeleteDocumentAsync_DoesNotThrow()
    {
        await _searchService.DeleteDocumentAsync(Guid.NewGuid());
    }
}
