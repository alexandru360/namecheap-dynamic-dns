using Microsoft.OpenApi.Models;
using NameCheapDynamicDns.Helpers;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

// Register Serilog
builder.Host.UseSerilog();

builder.Services.AddControllers();

builder.Services.AddSingleton(Log.Logger);
// Register DynDnsUpdater as a singleton
builder.Services.AddSingleton<IDynDnsUpdater, DynDnsUpdater>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc(VersionControl.APIVersion, new OpenApiInfo { Title = $"My API - v{VersionControl.APIVersion}", Version = VersionControl.APIVersion });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint($"swagger/{VersionControl.APIVersion}/swagger.json",$"My API - v{VersionControl.APIVersion}");
        options.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

app.MapGet("/get-dns-status", async ctx =>
    {
        var svc = ctx.RequestServices.GetRequiredService<IDynDnsUpdater>();
        await svc.UpdateDns();

        Results.Ok("Hello World!");
    })
    .WithOpenApi();

app.Run();
