using Qa2664.Localization;
using Volo.Abp.AspNetCore.Components;

namespace Qa2664.Blazor;

public abstract class Qa2664ComponentBase : AbpComponentBase
{
    protected Qa2664ComponentBase()
    {
        LocalizationResource = typeof(Qa2664Resource);
    }
}
