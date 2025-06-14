using Dashy.Net.Shared.Models;

namespace Dashy.Net.Web.Services;

public class DashboardStateService
{
    public DashboardConfigVm? Config { get; private set; }

    public event Action? OnConfigLoaded;

    public void SetConfig(DashboardConfigVm? config)
    {
        Config = config;
        OnConfigLoaded?.Invoke();
    }
}