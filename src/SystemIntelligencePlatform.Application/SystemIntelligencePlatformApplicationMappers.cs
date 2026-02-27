using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;
using SystemIntelligencePlatform.Books;
using SystemIntelligencePlatform.Incidents;
using SystemIntelligencePlatform.MonitoredApplications;

namespace SystemIntelligencePlatform;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SystemIntelligencePlatformBookToBookDtoMapper : MapperBase<Book, BookDto>
{
    public override partial BookDto Map(Book source);
    public override partial void Map(Book source, BookDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SystemIntelligencePlatformCreateUpdateBookDtoToBookMapper : MapperBase<CreateUpdateBookDto, Book>
{
    public override partial Book Map(CreateUpdateBookDto source);
    public override partial void Map(CreateUpdateBookDto source, Book destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class MonitoredApplicationToMonitoredApplicationDtoMapper : MapperBase<MonitoredApplication, MonitoredApplicationDto>
{
    public override partial MonitoredApplicationDto Map(MonitoredApplication source);
    public override partial void Map(MonitoredApplication source, MonitoredApplicationDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class IncidentToIncidentDtoMapper : MapperBase<Incident, IncidentDto>
{
    public override partial IncidentDto Map(Incident source);
    public override partial void Map(Incident source, IncidentDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class IncidentCommentToIncidentCommentDtoMapper : MapperBase<IncidentComment, IncidentCommentDto>
{
    public override partial IncidentCommentDto Map(IncidentComment source);
    public override partial void Map(IncidentComment source, IncidentCommentDto destination);
}
