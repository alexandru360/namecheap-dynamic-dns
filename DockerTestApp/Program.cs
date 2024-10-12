using DockerTestApp;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

Log.Information("Endpoints are being created ...");

// Map health check endpoint
app.MapHealthChecks("/health-check")
    .WithName("HealthCheck")
    .WithOpenApi();

app.MapGet("/", () => Results.Text("Application has started!"))
    .WithName("Default")
    .WithOpenApi();

app.MapGet("/assembly", () => Results.Json(Helper.GetAssemblyVersion()))
    .WithName("Assembly")
    .WithOpenApi();

app.MapGet("/console", () => Log.Information("Program is working"))
    .WithName("Print to console")
    .WithOpenApi();

Log.Information("Endpoints created.");
Log.Information("Endpoints:");
Log.Information("  - /health-check");
Log.Information("  - /assembly");
Log.Information("  - /console");

app.Run();
