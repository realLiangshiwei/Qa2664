using System;
using System.Collections.Generic;
using System.Text;
using Qa2664.Localization;
using Volo.Abp.Application.Services;

namespace Qa2664;

/* Inherit your application services from this class.
 */
public abstract class Qa2664AppService : ApplicationService
{
    protected Qa2664AppService()
    {
        LocalizationResource = typeof(Qa2664Resource);
    }
}
