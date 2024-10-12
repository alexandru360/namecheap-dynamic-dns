using DynDnsCronJob.Cron;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz.Spi;

namespace DynDnsCronJob;

public static class DynDnsCronJobConfiguration
{
    public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration config)
    {
        // Register the DynamicDnsUpdateJob
        services.AddSingleton<IJobFactory, SingletonJobFactory>();
        services.AddSingleton<DynamicDnsUpdateJob>();

        return services;
    }
}