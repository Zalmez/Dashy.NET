using Dashy.Net.Shared.Data;
using Microsoft.EntityFrameworkCore;

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
                
                // Ensure the database is created and migrations are applied
                await dbContext.Database.MigrateAsync(stoppingToken);
                
                logger.LogInformation("Database migration completed successfully");
                
                // Stop the application after migration
                hostApplicationLifetime.StopApplication();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during database migration.");
                hostApplicationLifetime.StopApplication();
            }
        }
    }
}
