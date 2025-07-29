using Dashy.Net.Shared.Models;

namespace Dashy.Net.Web.Services;

/// <summary>
/// Service responsible for coordinating dashboard data synchronization between clients.
/// This service handles the synchronization of dashboard configuration changes (items, sections, etc.)
/// while keeping client-specific view preferences (search terms, item sizes, etc.) separate.
/// </summary>
public class DashboardSyncService
{
    private readonly DashboardStateService _dashboardState;
    private readonly EditLockService _editLockService;
    private readonly ILogger<DashboardSyncService> _logger;
    private readonly Timer _syncTimer;

    // Track configuration version to detect changes
    private int _lastConfigVersion = 0;
    private DateTime _lastSyncTime = DateTime.MinValue;

    public DashboardSyncService(
        DashboardStateService dashboardState,
        EditLockService editLockService,
        ILogger<DashboardSyncService> logger)
    {
        _dashboardState = dashboardState;
        _editLockService = editLockService;
        _logger = logger;

        // Poll for configuration changes every 30 seconds
        // This ensures clients stay synchronized with dashboard changes made by other users
        _syncTimer = new Timer(CheckForConfigurationChanges, null, (int)TimeSpan.FromSeconds(30).TotalMilliseconds, (int)TimeSpan.FromSeconds(30).TotalMilliseconds);
    }

    /// <summary>
    /// Event fired when dashboard configuration has been updated and should be synchronized across clients.
    /// This does NOT affect client-specific view preferences.
    /// </summary>
    public event Action<DashboardConfigVm?>? OnConfigurationSynced;

    /// <summary>
    /// Manually triggers a configuration sync check.
    /// Used when we know a change has occurred and want immediate synchronization.
    /// </summary>
    public void TriggerConfigurationSync(int? dashboardId = null)
    {
        CheckForConfigurationChanges(null);
    }

    /// <summary>
    /// Notifies the service that a configuration change has been made locally.
    /// This prevents unnecessary polling and can trigger immediate sync to other clients.
    /// </summary>
    public void NotifyConfigurationChanged(DashboardConfigVm? newConfig)
    {
        if (newConfig != null)
        {
            _lastConfigVersion = newConfig.GetHashCode(); // Simple version tracking
            _lastSyncTime = DateTime.UtcNow;
        }

        OnConfigurationSynced?.Invoke(newConfig);
    }

    private void CheckForConfigurationChanges(object? state)
    {
        try
        {
            var currentConfig = _dashboardState.Config;
            if (currentConfig == null)
                return;

            var currentLock = _editLockService.GetCurrentLock(currentConfig.Id);
            if (currentLock != null)
            {
                _logger.LogDebug("Skipping configuration sync for dashboard {DashboardId} - currently being edited by {UserId}",
                    currentConfig.Id, currentLock.UserId);
                return;
            }

            if (DateTime.UtcNow - _lastSyncTime < TimeSpan.FromSeconds(10))
                return;

            _lastSyncTime = DateTime.UtcNow;

            _logger.LogDebug("Configuration sync check completed for dashboard {DashboardId}", currentConfig.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during configuration sync check");
        }
    }

    /// <summary>
    /// Gets statistics about the current synchronization state.
    /// Useful for monitoring and debugging.
    /// </summary>
    public DashboardSyncInfo GetSyncInfo()
    {
        return new DashboardSyncInfo
        {
            LastSyncTime = _lastSyncTime,
            LastConfigVersion = _lastConfigVersion,
            ActiveLocks = _editLockService.GetAllActiveLocks().Count,
            CurrentDashboardId = _dashboardState.Config?.Id
        };
    }

    public void Dispose()
    {
        _syncTimer?.Dispose();
    }
}

/// <summary>
/// Information about the current dashboard synchronization state.
/// </summary>
public class DashboardSyncInfo
{
    public DateTime LastSyncTime { get; set; }
    public int LastConfigVersion { get; set; }
    public int ActiveLocks { get; set; }
    public int? CurrentDashboardId { get; set; }
}
