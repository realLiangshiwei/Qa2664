using Qa2664.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace Qa2664;

[DependsOn(
    typeof(Qa2664EntityFrameworkCoreTestModule)
    )]
public class Qa2664DomainTestModule : AbpModule
{

}
