using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;
using SystemIntelligencePlatform.Alerting;
using SystemIntelligencePlatform.LogClusters;
using SystemIntelligencePlatform.LogEvents;
using SystemIntelligencePlatform.LogSources;
using SystemIntelligencePlatform.Copilot;
using SystemIntelligencePlatform.Incidents;
using SystemIntelligencePlatform.LogSearch;
using SystemIntelligencePlatform.MonitoredApplications;

namespace SystemIntelligencePlatform;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class MonitoredApplicationToMonitoredApplicationDtoMapper : MapperBase<MonitoredApplication, MonitoredApplicationDto>
{
    public override partial MonitoredApplicationDto Map(MonitoredApplication source);
    public override partial void Map(MonitoredApplication source, MonitoredApplicationDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class IncidentToIncidentDtoMapper : MapperBase<Incident, IncidentDto>
{
    /// <inheritdoc />
    /// <remarks>
    /// <see cref="IncidentDto.ApplicationName"/> is set in <see cref="IncidentAppService"/>.
    /// <see cref="IncidentDto.Comments"/> is loaded only in GetAsync; list queries must not map the aggregate collection.
    /// </remarks>
    [MapperIgnoreTarget(nameof(IncidentDto.ApplicationName))]
    [MapperIgnoreTarget(nameof(IncidentDto.Comments))]
    [MapperIgnoreTarget(nameof(IncidentDto.AssigneeUserName))]
    [MapperIgnoreTarget(nameof(IncidentDto.MergedChildIncidentIds))]
    public override partial IncidentDto Map(Incident source);

    [MapperIgnoreTarget(nameof(IncidentDto.ApplicationName))]
    [MapperIgnoreTarget(nameof(IncidentDto.Comments))]
    [MapperIgnoreTarget(nameof(IncidentDto.AssigneeUserName))]
    [MapperIgnoreTarget(nameof(IncidentDto.MergedChildIncidentIds))]
    public override partial void Map(Incident source, IncidentDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class IncidentCommentToIncidentCommentDtoMapper : MapperBase<IncidentComment, IncidentCommentDto>
{
    public override partial IncidentCommentDto Map(IncidentComment source);
    public override partial void Map(IncidentComment source, IncidentCommentDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class CopilotConversationMessageToCopilotMessageDtoMapper
    : MapperBase<CopilotConversationMessage, CopilotMessageDto>
{
    public override partial CopilotMessageDto Map(CopilotConversationMessage source);
    public override partial void Map(CopilotConversationMessage source, CopilotMessageDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SavedLogSearchToSavedLogSearchDtoMapper : MapperBase<SavedLogSearch, SavedLogSearchDto>
{
    public override partial SavedLogSearchDto Map(SavedLogSearch source);
    public override partial void Map(SavedLogSearch source, SavedLogSearchDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class AlertRuleToAlertRuleDtoMapper : MapperBase<AlertRule, AlertRuleDto>
{
    public override partial AlertRuleDto Map(AlertRule source);
    public override partial void Map(AlertRule source, AlertRuleDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class AlertHistoryToAlertHistoryDtoMapper : MapperBase<AlertHistory, AlertHistoryDto>
{
    public override partial AlertHistoryDto Map(AlertHistory source);
    public override partial void Map(AlertHistory source, AlertHistoryDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class LogSourceConfigurationToDtoMapper : MapperBase<LogSourceConfiguration, LogSourceConfigurationDto>
{
    public override partial LogSourceConfigurationDto Map(LogSourceConfiguration source);
    public override partial void Map(LogSourceConfiguration source, LogSourceConfigurationDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class LogClusterToLogClusterDtoMapper : MapperBase<LogCluster, LogClusterDto>
{
    public override partial LogClusterDto Map(LogCluster source);
    public override partial void Map(LogCluster source, LogClusterDto destination);
}
