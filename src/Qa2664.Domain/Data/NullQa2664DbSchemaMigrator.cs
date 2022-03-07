using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Qa2664.Data;

/* This is used if database provider does't define
 * IQa2664DbSchemaMigrator implementation.
 */
public class NullQa2664DbSchemaMigrator : IQa2664DbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
