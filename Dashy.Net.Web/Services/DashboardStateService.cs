using Dashy.Net.Shared.Models;

namespace Dashy.Net.Web.Services;

/// <summary>
/// Manages global dashboard configuration state that should be synchronized across all clients.
/// This includes the actual dashboard data (sections, items, etc.) but NOT client-specific UI preferences.
/// </summary>
public class DashboardStateService
{
    public DashboardConfigVm? Config { get; private set; }

    public event Action? OnConfigLoaded;

    /// <summary>
    /// Updates the dashboard configuration. This will trigger updates across all connected clients
    /// for the dashboard data, but not for client-specific view preferences.
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