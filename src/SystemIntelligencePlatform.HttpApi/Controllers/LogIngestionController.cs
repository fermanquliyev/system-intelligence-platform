using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SystemIntelligencePlatform.LogIngestion;

namespace SystemIntelligencePlatform.Controllers;

/// <summary>
/// Public ingestion endpoint - authenticated via API key header, not OAuth.
/// API Management routes external traffic here.
/// </summary>
[Route("api/ingest")]
[AllowAnonymous]
public class LogIngestionController : SystemIntelligencePlatformController
{
    private readonly LogIngestionAppService _logIngestionAppService;

    public LogIngestionController(LogIngestionAppService logIngestionAppService)
    {
        _logIngestionAppService = logIngestionAppService;
    }

    [HttpPost]
    public async Task<LogIngestionResultDto> IngestAsync(
        [FromHeader(Name = "X-Api-Key")] string apiKey,
        [FromBody] LogIngestionDto input)
    {
        return await _logIngestionAppService.IngestAsync(apiKey, input);
    }
}
