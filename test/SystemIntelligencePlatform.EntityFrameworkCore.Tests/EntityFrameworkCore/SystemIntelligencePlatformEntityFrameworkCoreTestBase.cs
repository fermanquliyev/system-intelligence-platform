using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace SystemIntelligencePlatform.EntityFrameworkCore;

public abstract class SystemIntelligencePlatformEntityFrameworkCoreTestBase : SystemIntelligencePlatformTestBase<SystemIntelligencePlatformEntityFrameworkCoreTestModule>
{
    /// <summary>
    /// Disables the multi-tenancy filter so entities with TenantId=null are visible in host context.
    /// ABP's filter otherwise hides entities when TenantId doesn't match CurrentTenant.Id.
    /// </summary>
    protected override async Task WithUnitOfWorkAsync(AbpUnitOfWorkOptions options, Func<Task> action)
    {
        using (GetRequiredService<IDataFilter>().Disable<IMultiTenant>())
        {
            await base.WithUnitOfWorkAsync(options, action);
        }
    }

    protected override async Task<TResult> WithUnitOfWorkAsync<TResult>(AbpUnitOfWorkOptions options, Func<Task<TResult>> func)
    {
        using (GetRequiredService<IDataFilter>().Disable<IMultiTenant>())
        {
            return await base.WithUnitOfWorkAsync(options, func);
        }
    }
}
