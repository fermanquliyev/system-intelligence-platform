using System;
using System.Threading.Tasks;
using Shouldly;
using SystemIntelligencePlatform.Incidents;
using Xunit;

namespace SystemIntelligencePlatform.EntityFrameworkCore.Incidents;

/// <summary>
/// Tests for the EF Core implementation of IIncidentRepository.
/// Verifies query methods for finding incidents by hash signature, getting active incidents,
/// and calculating severity distributions.
/// </summary>
[Collection(SystemIntelligencePlatformTestConsts.CollectionDefinitionName)]
public class EfCoreIncidentRepository_Tests : SystemIntelligencePlatformEntityFrameworkCoreTestBase
{
    private readonly IIncidentRepository _incidentRepository;

    public EfCoreIncidentRepository_Tests()
    {
        _incidentRepository = GetRequiredService<IIncidentRepository>();
    }

    [Fact]
    public async Task Should_Find_By_HashSignature_Only_Active()
    {
        var applicationId = Guid.NewGuid();
        var openHash = "test-hash-open";
        var resolvedHash = "test-hash-resolved";

        await WithUnitOfWorkAsync(async () =>
        {
            var openIncident = new Incident(
                Guid.NewGuid(), applicationId, "Open Incident",
                openHash, IncidentSeverity.Medium, DateTime.UtcNow);

            var resolvedIncident = new Incident(
                Guid.NewGuid(), applicationId, "Resolved Incident",
                resolvedHash, IncidentSeverity.Low, DateTime.UtcNow.AddDays(-10));
            resolvedIncident.Resolve(Guid.NewGuid());

            await _incidentRepository.InsertAsync(openIncident);
            await _incidentRepository.InsertAsync(resolvedIncident);
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var foundOpen = await _incidentRepository.FindByHashSignatureAsync(openHash, applicationId);
            foundOpen.ShouldNotBeNull();
            foundOpen.Status.ShouldBe(IncidentStatus.Open);

            var foundResolved = await _incidentRepository.FindByHashSignatureAsync(resolvedHash, applicationId);
            foundResolved.ShouldBeNull("Resolved incidents should not be returned");
        });
    }

    [Fact]
    public async Task Should_Return_Active_Incidents_Ordered_By_LastOccurrence()
    {
        var applicationId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        Guid id1 = Guid.Empty, id2 = Guid.Empty, id3 = Guid.Empty;

        await WithUnitOfWorkAsync(async () =>
        {
            var incident1 = new Incident(
                Guid.NewGuid(), applicationId, "Incident 1",
                "hash-ord-1", IncidentSeverity.Low, now.AddHours(-5));
            id1 = incident1.Id;

            var incident2 = new Incident(
                Guid.NewGuid(), applicationId, "Incident 2",
                "hash-ord-2", IncidentSeverity.Medium, now.AddHours(-2));
            incident2.IncrementOccurrence(now.AddHours(-1));
            id2 = incident2.Id;

            var incident3 = new Incident(
                Guid.NewGuid(), applicationId, "Incident 3",
                "hash-ord-3", IncidentSeverity.High, now.AddHours(-10));
            incident3.IncrementOccurrence(now.AddMinutes(-30));
            id3 = incident3.Id;

            await _incidentRepository.InsertAsync(incident1);
            await _incidentRepository.InsertAsync(incident2);
            await _incidentRepository.InsertAsync(incident3);
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var active = await _incidentRepository.GetActiveIncidentsAsync(applicationId, 10);
            active.Count.ShouldBe(3);
            // incident3 has LastOccurrence = now-30min (most recent), incident2 = now-1h, incident1 = now-5h
            active[0].Id.ShouldBe(id3);
            active[1].Id.ShouldBe(id2);
            active[2].Id.ShouldBe(id1);
        });
    }

    [Fact]
    public async Task Should_Calculate_Severity_Distribution()
    {
        var applicationId = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            await _incidentRepository.InsertAsync(new Incident(
                Guid.NewGuid(), applicationId, "Low 1", "hash-dist-1", IncidentSeverity.Low, DateTime.UtcNow));
            await _incidentRepository.InsertAsync(new Incident(
                Guid.NewGuid(), applicationId, "Low 2", "hash-dist-2", IncidentSeverity.Low, DateTime.UtcNow));
            await _incidentRepository.InsertAsync(new Incident(
                Guid.NewGuid(), applicationId, "Med 1", "hash-dist-3", IncidentSeverity.Medium, DateTime.UtcNow));

            var resolved = new Incident(
                Guid.NewGuid(), applicationId, "Crit Resolved", "hash-dist-4", IncidentSeverity.Critical, DateTime.UtcNow);
            resolved.Resolve(Guid.NewGuid());
            await _incidentRepository.InsertAsync(resolved);
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var dist = await _incidentRepository.GetSeverityDistributionAsync(applicationId);
            dist.ShouldNotBeNull();
            dist[IncidentSeverity.Low].ShouldBe(2);
            dist[IncidentSeverity.Medium].ShouldBe(1);
            dist.ContainsKey(IncidentSeverity.Critical).ShouldBeFalse();
        });
    }

    [Fact]
    public async Task Should_Respect_MaxCount_Parameter()
    {
        var applicationId = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            for (int i = 0; i < 10; i++)
            {
                await _incidentRepository.InsertAsync(new Incident(
                    Guid.NewGuid(), applicationId, $"Incident {i}",
                    $"hash-max-{i}", IncidentSeverity.Low, DateTime.UtcNow.AddMinutes(-i)));
            }
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var result = await _incidentRepository.GetActiveIncidentsAsync(applicationId, 5);
            result.Count.ShouldBe(5);
        });
    }
}
