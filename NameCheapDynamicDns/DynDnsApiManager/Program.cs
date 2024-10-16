using DynDnsCronJob.Cron;
using DynDnsCronJob.Models;
using DynDnsDynamicLibrary;
using DynDnsDynamicLibrary.Config;
using Serilog;
using Serilog.Events;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Configure Serilog
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .CreateLogger();

    builder.Host.UseSerilog(); // Register Serilog

    builder.Services.AddSingleton(Log.Logger);

    // Enable configuration reload on change
    builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true,
        reloadOnChange: true);

    builder.Services.Configure<NamecheapConfig>(
        builder.Configuration.GetSection("NamecheapConfig")); // Configure NamecheapConfig options
    builder.Services.Configure<CronJobConfig>(
        builder.Configuration.GetSection("CronJob")); // Configure CronJobConfig options

    builder.Services.AddConfig(builder.Configuration); // Register services from DynDnsDynamicLibrary

    builder.Services.AddControllers().AddNewtonsoftJson();

    // Add Worker service
    builder.Services.AddHostedService<DynamicDnsWorker>();

    // Add services to the container.
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    app.UseSerilogRequestLogging(options =>
    {
        options.GetLevel = (httpContext, elapsed, ex) =>
        {
            if (ex != null || httpContext.Response.StatusCode > 499)
            {
                return LogEventLevel.Error;
            }
            else if (httpContext.Response.StatusCode > 399)
            {
                return LogEventLevel.Warning;
            }

            return LogEventLevel.Information;
        };
    });

    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseHttpsRedirection();

    app.MapGet("/get-dns-status",
            async (IDynamicDnsHelper svc) =>
            {
                await svc.UpdateDns();
                return Results.Ok("Dns update process executed; check the logs!");
            })
        .WithOpenApi();

    app.Run();
}
catch (Exception ex)
{
    Log.Error(ex, "An unhandled exception occurred.");
}
finally
{
    Log.CloseAndFlush();
}