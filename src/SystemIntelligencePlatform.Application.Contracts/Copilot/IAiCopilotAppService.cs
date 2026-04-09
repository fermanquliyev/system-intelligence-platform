using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SystemIntelligencePlatform.Copilot;

public interface IAiCopilotAppService : IApplicationService
{
    Task<CopilotAnalysisDto> GetAnalysisAsync(Guid incidentId);

    Task<CopilotMessageDto> PostFollowUpAsync(Guid incidentId, CopilotFollowUpInput input);

    Task<IReadOnlyList<CopilotMessageDto>> GetConversationAsync(Guid incidentId);
}
