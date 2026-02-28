using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SystemIntelligencePlatform.LogIngestion;

public interface ILogIngestionAppService : IApplicationService
{
    Task<LogIngestionResultDto> IngestAsync(string apiKey, LogIngestionDto input);
}
