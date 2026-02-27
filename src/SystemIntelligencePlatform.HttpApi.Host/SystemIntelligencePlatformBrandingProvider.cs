using Microsoft.Extensions.Localization;
using SystemIntelligencePlatform.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace SystemIntelligencePlatform;

[Dependency(ReplaceServices = true)]
public class SystemIntelligencePlatformBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<SystemIntelligencePlatformResource> _localizer;

    public SystemIntelligencePlatformBrandingProvider(IStringLocalizer<SystemIntelligencePlatformResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
