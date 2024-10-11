using DynDnsDynamicLibrary;
using DynDnsDynamicLibrary.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog(); // Register Serilog

builder.Services.AddSingleton(Log.Logger);

builder.Services.Configure<NamecheapConfig>(builder.Configuration.GetSection("NamecheapConfig")); // Configure NamecheapConfig options
builder.Services.AddConfig(builder.Configuration); // Register services from DynDnsDynamicLibrary

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/get-dns-status", async (IDynamicDnsHelper svc) =>
    {
        var response = await svc.UpdateDns();

        return Results.Ok(response);
    })
    .WithOpenApi();

app.Run();
