using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace SystemIntelligencePlatform.InstanceConfiguration;

public class InstanceConfigurationDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<InstanceFeature, Guid> _featureRepository;

    public InstanceConfigurationDataSeedContributor(IRepository<InstanceFeature, Guid> featureRepository)
    {
        _featureRepository = featureRepository;
    }

    [UnitOfWork]
    public async Task SeedAsync(DataSeedContext context)
    {
        var existing = await _featureRepository.GetListAsync();
        var existingNames = existing.Select(f => f.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var seed in InstanceConfigurationRegistry.FeatureSeeds)
        {
            if (existingNames.Contains(seed.Name))
                continue;

            await _featureRepository.InsertAsync(new InstanceFeature(
                Guid.NewGuid(),
                seed.Name,
                seed.DisplayName,
                seed.Description,
                seed.DefaultEnabled,
                seed.Order));
        }
    }
}
