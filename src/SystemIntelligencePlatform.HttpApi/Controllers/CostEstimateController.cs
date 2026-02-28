using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SystemIntelligencePlatform.CostEstimation;

namespace SystemIntelligencePlatform.Controllers;

[Route("api/app/cost-estimate")]
[Authorize]
public class CostEstimateController : SystemIntelligencePlatformController
{
    private readonly ICostEstimatorAppService _costEstimator;

    public CostEstimateController(ICostEstimatorAppService costEstimator)
    {
        _costEstimator = costEstimator;
    }

    [HttpGet]
    public CostEstimateDto Get([FromQuery] long logsPerDay = 1000000, [FromQuery] bool aiEnabled = true)
    {
        return _costEstimator.Calculate(new CostEstimateInput
        {
            LogsPerDay = logsPerDay,
            AiEnrichmentEnabled = aiEnabled
        });
    }
}
