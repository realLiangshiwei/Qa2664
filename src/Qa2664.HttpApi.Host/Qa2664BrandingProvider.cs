using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace Qa2664;

[Dependency(ReplaceServices = true)]
public class Qa2664BrandingProvider : DefaultBrandingProvider
{
    public override string AppName => "Qa2664";
}
