using System.Threading.Tasks;

namespace Qa2664.Data;

public interface IQa2664DbSchemaMigrator
{
    Task MigrateAsync();
}
