using Volo.Abp.Application.Services;

namespace SystemIntelligencePlatform.CostEstimation;

public interface ICostEstimatorAppService : IApplicationService
{
    CostEstimateDto Calculate(CostEstimateInput input);
}
