using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qa2664.Data;
using Volo.Abp.DependencyInjection;

namespace Qa2664.EntityFrameworkCore;

public class EntityFrameworkCoreQa2664DbSchemaMigrator
    : IQa2664DbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreQa2664DbSchemaMigrator(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the Qa2664DbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<Qa2664DbContext>()
            .Database
            .MigrateAsync();
    }
}
