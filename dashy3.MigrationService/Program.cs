using dashy3.ApiService.Data;
using dashy3.MigrationService;

var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddHostedService<Worker>();
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(Worker.ActivitySourceName));

builder.AddNpgsqlDbContext<DashboardDbContext>("dashy3db");

var host = builder.Build();
host.Run();
