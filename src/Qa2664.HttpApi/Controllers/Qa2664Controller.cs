using Qa2664.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace Qa2664.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class Qa2664Controller : AbpControllerBase
{
    protected Qa2664Controller()
    {
        LocalizationResource = typeof(Qa2664Resource);
    }
}
