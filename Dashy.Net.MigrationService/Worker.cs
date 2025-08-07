using Dashy.Net.Shared.Data;
using Dashy.Net.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Diagnostics;
using System.Text;

namespace Dashy.Net.MigrationService;

public class Worker(IServiceProvider serviceProvider, IHostApplicationLifetime hostApplicationLifetime, ILogger<Worker> logger) : BackgroundService
{
    public const string ActivitySourceName = "Migrations";
    private static readonly ActivitySource s_activitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var activity = s_activitySource.StartActivity("Migrating database", ActivityKind.Client);

        try
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await RunMigrationAsync(dbContext, cancellationToken);
            await SeedDataAsync(dbContext, cancellationToken);
            
        }
        catch (Exception)
        {
            Environment.ExitCode = 1;
            throw;
        }

        hostApplicationLifetime.StopApplication();
    }
    private static async Task RunMigrationAsync(DbContext dbContext, CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        });
    }

    private static async Task SeedDataAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
        if (await dbContext.Dashboards.AnyAsync(cancellationToken))
        {
            return;
        }

        var dashboard = new Dashboard
        {
            Title = "Dashy.Net Home Lab",
            Subtitle = "Your new dashboard, ready to go!",
            HeaderButtons =
            [
                new HeaderButton { Text = "GitHub", Url = "https://github.com/Zalmez/Dashy.NET", Position = 0 },
                new HeaderButton { Text = "Documentation", Url = "https://dashy.to/docs", Position = 1 }
            ],
            Sections =
            [
                new DashboardSection
                {
                    Name = "Networking",
                    Icon = "fas fa-network-wired",
                    Position = 0,
                    Items =
                    [
                        new DashboardItem { Title = "Router", Widget = "static-link", Url = "#", Icon = "fas fa-road-bridge", Position = 0 },
                        new DashboardItem { Title = "Pi-hole", Widget = "static-link", Url = "#", Icon = "fas fa-shield-alt", Position = 1 }
                    ]
                },
                new DashboardSection
                {
                    Name = "Media Servers",
                    Icon = "fas fa-photo-film",
                    Position = 1,
                    Items =
                    [
                        new DashboardItem { Title = "Jellyfin", Widget = "static-link", Url = "#", Icon = "fas fa-tv", Position = 0 }
                    ]
                }
            ]
        };

        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            await dbContext.Dashboards.AddAsync(dashboard, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });
    }
    // protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    // {
    //     logger.LogInformation("Migration worker running: {time}", DateTimeOffset.Now);
    //     try
    //     {
    //         using var scope = serviceProvider.CreateScope();
    //         var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    //         const int maxAttempts = 5;
    //         const int delaySeconds = 60;
    //         int attempt = 0;
    //         bool dbAvailable = false;
    //         var exceptions = new List<Exception>();

    //         while (attempt < maxAttempts && !dbAvailable && !stoppingToken.IsCancellationRequested)
    //         {
    //             try
    //             {
    //                 await dbContext.Database.OpenConnectionAsync(stoppingToken);
    //                 await dbContext.Database.CloseConnectionAsync();
    //                 dbAvailable = true;
    //             }
    //             catch (Exception ex)
    //             {
    //                 exceptions.Add(ex);
    //                 attempt++;
    //                 if (attempt == 1)
    //                 {
    //                     logger.LogWarning(ex, "Unable to reach the database on first attempt.");
    //                 }
    //                 else if (attempt < maxAttempts)
    //                 {
    //                     logger.LogInformation("Waiting for the database to be available (retrying in {Delay}s, attempt {Attempt}/{MaxAttempts})...", delaySeconds, attempt, maxAttempts);
    //                     await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);
    //                 }
    //             }
    //         }

    //         if (!dbAvailable)
    //         {
    //             logger.LogError("Database was out of reach after {MaxAttempts} attempts. See below for all connection errors:", maxAttempts);
    //             var sb = new StringBuilder();
    //             for (int i = 0; i < exceptions.Count; i++)
    //             {
    //                 sb.AppendLine($"--- Attempt {i + 1} ---");
    //                 sb.AppendLine(exceptions[i].ToString());
    //             }
    //             logger.LogError(sb.ToString());
    //             Environment.ExitCode = 1; // Set exit code for failure
    //             throw new AggregateException($"Database was out of reach after {maxAttempts} attempts.", exceptions);
    //         }

    //         await dbContext.Database.MigrateAsync(stoppingToken);
    //         logger.LogInformation("Database migration completed successfully");

    //         // Gracefully stop the application after successful execution
    //         logger.LogInformation("Stopping application gracefully after successful migration.");
    //         Environment.ExitCode = 0; // Set exit code for success
    //         hostApplicationLifetime.StopApplication();
    //     }
    //     catch (Exception ex)
    //     {
    //         logger.LogError(ex, "An error occurred during database migration.");
    //         Environment.ExitCode = 1; // Set exit code for failure
    //         hostApplicationLifetime.StopApplication();
    //     }
    // }
}