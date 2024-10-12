using DynDnsCronJob.Cron;
using Quartz;
using Quartz.Impl;

namespace DynDnsCronJob;

public class DynDnsCronJobConProgram
{
    private static async Task Main(string[] args)
    {
        // Grab the Scheduler instance from the Factory
        StdSchedulerFactory factory = new StdSchedulerFactory();
        IScheduler scheduler = await factory.GetScheduler();

        // and start it off
        await scheduler.Start();

        // define the job and tie it to our HelloJob class
        IJobDetail job = JobBuilder.Create<DynamicDnsUpdateJob>()
            .WithIdentity("job1", "group1")
            .Build();

        // Trigger the job to run now, and then repeat every 10 seconds
        ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity("trigger1", "group1")
            .StartNow()
            .WithSimpleSchedule(x => x
                .WithIntervalInMinutes(15)
                .RepeatForever())
            .Build();

        // Tell Quartz to schedule the job using our trigger
        await scheduler.ScheduleJob(job, trigger);

        // some sleep to show what's happening
        // await Task.Delay(TimeSpan.FromSeconds(60));

        // and last shut down the scheduler when you are ready to close your program
        await scheduler.Shutdown();
    }
}