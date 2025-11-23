using Dashy.Net.Shared.Models;
using Dashy.Net.Web.Logging;

namespace Dashy.Net.Web.Services;

/// <summary>
/// Manages client-specific dashboard configuration state. Each client connection has its own instance.
/// This includes the actual dashboard data (sections, items, etc.) but NOT client-specific UI preferences.
/// </summary>
public class DashboardStateService
{
    private readonly ILogger<DashboardStateService> _logger;
    private readonly TimeProvider _timeProvider;

    public DashboardStateService(ILogger<DashboardStateService> logger, TimeProvider timeProvider)
    {
        _logger = logger;
        _timeProvider = timeProvider;
    }

    public DashboardConfigVm? Config { get; private set; }

    public event Action? OnConfigLoaded;

    /// <summary>
    /// Updates the dashboard configuration. This will only affect the current client.
    /// Dashboard data synchronization across clients is handled separately by DashboardSyncService.
    /// </summary>
    /// <param name="config">The new dashboard configuration</param>
    public void SetConfig(DashboardConfigVm? config)
    {
        var start = _timeProvider.GetTimestamp();
        Config = config;
        OnConfigLoaded?.Invoke();
        if (config is null)
        {
            Log.DashboardMissingConfig(_logger);
        }
        else
        {
            var sectionCount = config.Sections?.Count ?? 0;
            var itemCount = config.Sections?.Sum(s => s.Items?.Count ?? 0) ?? 0;
            var elapsedMs = (long)((_timeProvider.GetTimestamp() - start) / (double)_timeProvider.TimestampFrequency * 1000);
            Log.DashboardLoaded(_logger, sectionCount, itemCount, elapsedMs);
        }
    }

    /// <summary>
    /// Clears the current dashboard configuration
    /// </summary>
    public void ClearConfig()
    {
        Config = null;
        OnConfigLoaded?.Invoke();
        Log.DashboardMissingConfig(_logger);
    }
}