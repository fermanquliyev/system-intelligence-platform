using SystemIntelligencePlatform.Localization;
using Volo.Abp.Application.Services;

namespace SystemIntelligencePlatform;

/* Inherit your application services from this class.
 */
public abstract class SystemIntelligencePlatformAppService : ApplicationService
{
    protected SystemIntelligencePlatformAppService()
    {
        LocalizationResource = typeof(SystemIntelligencePlatformResource);
    }
}
