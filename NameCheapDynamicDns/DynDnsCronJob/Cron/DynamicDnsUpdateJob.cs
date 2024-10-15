using DynDnsCronJob.Models;
using DynDnsDynamicLibrary;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace DynDnsCronJob.Cron;

public class DynamicDnsWorker(
    IDynamicDnsHelper dynamicDnsHelper,
    IOptions<CronJobConfig> config) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(async () =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await dynamicDnsHelper.UpdateDns();

                var time = config.Value.UpdateInMinutes;
                await Task.Delay(TimeSpan.FromMinutes(time), stoppingToken);
            }
        }, stoppingToken);
    }
}