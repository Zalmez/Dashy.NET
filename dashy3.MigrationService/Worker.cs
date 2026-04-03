using System.Diagnostics;
using dashy3.ApiService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using OpenTelemetry.Trace;

namespace dashy3.MigrationService;

public class Worker(
    IServiceProvider serviceProvider,
    IHostApplicationLifetime hostApplicationLifetime) : BackgroundService
{
    public const string ActivitySourceName = "Migrations";
    private static readonly ActivitySource s_activitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var activity = s_activitySource.StartActivity("Migrating database", ActivityKind.Client);
        try
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DashboardDbContext>();
            await RunMigrationAsync(dbContext, cancellationToken);
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
            throw;
        }

        hostApplicationLifetime.StopApplication();
    }

    private static async Task RunMigrationAsync(DashboardDbContext dbContext, CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            // If the database already has tables but no migration history (created via EnsureCreated),
            // mark all currently defined migrations as applied so MigrateAsync won't try to re-create them.
            await EnsureMigrationHistoryAsync(dbContext, cancellationToken);
            await dbContext.Database.MigrateAsync(cancellationToken);
        });
    }

    private static async Task EnsureMigrationHistoryAsync(DashboardDbContext dbContext, CancellationToken cancellationToken)
    {
        // Check whether the migrations history table exists yet.
        var historyRepo = dbContext.GetService<IHistoryRepository>();
        if (await historyRepo.ExistsAsync(cancellationToken))
            return; // History table present — MigrateAsync handles the rest normally.

        // History table doesn't exist but tables might already be there (EnsureCreated legacy).
        // Check for a known table to detect an existing untracked schema.
        var conn = dbContext.Database.GetDbConnection();
        await conn.OpenAsync(cancellationToken);
        bool tablesExist;
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'AspNetRoles')";
            tablesExist = (bool)(await cmd.ExecuteScalarAsync(cancellationToken))!;
        }
        await conn.CloseAsync();

        if (!tablesExist)
            return; // Fresh database — let MigrateAsync create everything from scratch.

        // Existing untracked database: create the history table and mark every known
        // migration as already applied so MigrateAsync only runs truly new ones.
        await historyRepo.CreateIfNotExistsAsync(cancellationToken);

        var appliedMigrations = (await historyRepo.GetAppliedMigrationsAsync(cancellationToken))
            .Select(r => r.MigrationId)
            .ToHashSet();

        foreach (var migrationId in dbContext.Database.GetMigrations())
        {
            if (!appliedMigrations.Contains(migrationId))
            {
                var sql = historyRepo.GetInsertScript(new HistoryRow(migrationId, "8.0.0"));
                await dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
            }
        }
    }
}
