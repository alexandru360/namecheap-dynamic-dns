using DynDnsDynamicLibrary;
using Quartz;

namespace DynDnsCronJob.Cron;

public class DynamicDnsUpdateJob(IDynamicDnsHelper dynamicDnsHelper) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        await dynamicDnsHelper.UpdateDns();
    }
}