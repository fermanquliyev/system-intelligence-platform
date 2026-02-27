using SystemIntelligencePlatform.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace SystemIntelligencePlatform.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class SystemIntelligencePlatformController : AbpControllerBase
{
    protected SystemIntelligencePlatformController()
    {
        LocalizationResource = typeof(SystemIntelligencePlatformResource);
    }
}
