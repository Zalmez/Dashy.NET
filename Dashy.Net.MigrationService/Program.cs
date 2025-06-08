using Dashy.Net.Shared.Data;
using Dashy.Net.MigrationService;

var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<AppDbContext>("dashy");
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
