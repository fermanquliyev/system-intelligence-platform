using SystemIntelligencePlatform.Samples;
using Xunit;

namespace SystemIntelligencePlatform.EntityFrameworkCore.Domains;

[Collection(SystemIntelligencePlatformTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<SystemIntelligencePlatformEntityFrameworkCoreTestModule>
{

}
