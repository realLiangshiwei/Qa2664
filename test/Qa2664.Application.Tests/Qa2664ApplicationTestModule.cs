using Volo.Abp.Modularity;

namespace Qa2664;

[DependsOn(
    typeof(Qa2664ApplicationModule),
    typeof(Qa2664DomainTestModule)
    )]
public class Qa2664ApplicationTestModule : AbpModule
{

}
