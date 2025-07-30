using Dashy.Net.Shared.Models;

namespace Dashy.Net.Web.Services;

/// <summary>
/// Manages client-specific dashboard configuration state. Each client connection has its own instance.
/// This includes the actual dashboard data (sections, items, etc.) but NOT client-specific UI preferences.
/// </summary>
public class DashboardStateService
{
    public DashboardConfigVm? Config { get; private set; }

    public event Action? OnConfigLoaded;

    /// <summary>
    /// Updates the dashboard configuration. This will only affect the current client.
    /// Dashboard data synchronization across clients is handled separately by DashboardSyncService.
    /// </summary>
    /// <param name="config">The new dashboard configuration</param>
    public void SetConfig(DashboardConfigVm? config)
    {
        Config = config;
        OnConfigLoaded?.Invoke();
    }

    /// <summary>
    /// Clears the current dashboard configuration
    /// </summary>
    public void ClearConfig()
    {
        Config = null;
        OnConfigLoaded?.Invoke();
    }
}