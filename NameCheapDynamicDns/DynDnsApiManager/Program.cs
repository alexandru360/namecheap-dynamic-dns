using DynDnsDynamicLibrary;
using DynDnsDynamicLibrary.Config;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

// Register Serilog
builder.Host.UseSerilog();

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
        await svc.UpdateDns();

        return Results.Ok("Hello World!");
    })
    .WithOpenApi();

app.Run();
