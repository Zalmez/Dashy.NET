using Dashy.Net.Shared.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Text;

namespace Dashy.Net.MigrationService
{
    public class Worker(IServiceProvider serviceProvider, IHostApplicationLifetime hostApplicationLifetime, ILogger<Worker> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Migration worker running: {time}", DateTimeOffset.Now);
            try
            {
                using var scope = serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                const int maxAttempts = 5;
                const int delaySeconds = 60;
                int attempt = 0;
                bool dbAvailable = false;
                var exceptions = new List<Exception>();

                while (attempt < maxAttempts && !dbAvailable && !stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        await dbContext.Database.OpenConnectionAsync(stoppingToken);
                        await dbContext.Database.CloseConnectionAsync();
                        dbAvailable = true;
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                        attempt++;
                        if (attempt == 1)
                        {
                            logger.LogWarning(ex, "Unable to reach the database on first attempt.");
                        }
                        else if (attempt < maxAttempts)
                        {
                            logger.LogInformation("Waiting for the database to be available (retrying in {Delay}s, attempt {Attempt}/{MaxAttempts})...", delaySeconds, attempt, maxAttempts);
                            await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);
                        }
                    }
                }

                if (!dbAvailable)
                {
                    logger.LogError("Database was out of reach after {MaxAttempts} attempts. See below for all connection errors:", maxAttempts);
                    var sb = new StringBuilder();
                    for (int i = 0; i < exceptions.Count; i++)
                    {
                        sb.AppendLine($"--- Attempt {i + 1} ---");
                        sb.AppendLine(exceptions[i].ToString());
                    }
                    logger.LogError(sb.ToString());
                    Environment.ExitCode = 1; // Set exit code for failure
                    throw new AggregateException($"Database was out of reach after {maxAttempts} attempts.", exceptions);
                }

                await dbContext.Database.MigrateAsync(stoppingToken);
                logger.LogInformation("Database migration completed successfully");

                // Gracefully stop the application after successful execution
                logger.LogInformation("Stopping application gracefully after successful migration.");
                Environment.ExitCode = 0; // Set exit code for success
                hostApplicationLifetime.StopApplication();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during database migration.");
                Environment.ExitCode = 1; // Set exit code for failure
                hostApplicationLifetime.StopApplication();
            }
        }
    }
}
