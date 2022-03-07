using Qa2664.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Modularity;

namespace Qa2664.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(Qa2664EntityFrameworkCoreModule),
    typeof(Qa2664ApplicationContractsModule)
    )]
public class Qa2664DbMigratorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpBackgroundJobOptions>(options => options.IsJobExecutionEnabled = false);
    }
}
