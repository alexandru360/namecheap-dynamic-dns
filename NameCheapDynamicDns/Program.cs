using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

// Register Serilog
builder.Host.UseSerilog();

// Register DynDnsUpdater as a singleton
builder.Services.AddSingleton<DynDnsUpdater>(provider =>
{
    var logger = provider.GetRequiredService<Serilog.ILogger>();
    return new DynDnsUpdater(logger);
});

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

app.MapGet("/get-dns-status", () =>
    {
        return "Hello World!";
    })
    .WithName("GetDnsStatus")
    .WithOpenApi();

app.Run();
