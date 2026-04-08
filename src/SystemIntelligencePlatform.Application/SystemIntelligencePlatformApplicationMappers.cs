using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;
using SystemIntelligencePlatform.Incidents;
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
    public override partial IncidentDto Map(Incident source);

    [MapperIgnoreTarget(nameof(IncidentDto.ApplicationName))]
    [MapperIgnoreTarget(nameof(IncidentDto.Comments))]
    public override partial void Map(Incident source, IncidentDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class IncidentCommentToIncidentCommentDtoMapper : MapperBase<IncidentComment, IncidentCommentDto>
{
    public override partial IncidentCommentDto Map(IncidentComment source);
    public override partial void Map(IncidentComment source, IncidentCommentDto destination);
}
