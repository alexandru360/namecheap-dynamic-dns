using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;

namespace DynDnsCronJob.Cron;

public class SingletonJobFactory(IServiceProvider serviceProvider) : IJobFactory
{
    public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
    {
        return serviceProvider.GetRequiredService(bundle.JobDetail.JobType) as IJob;
    }

    public void ReturnJob(IJob job) { }
}