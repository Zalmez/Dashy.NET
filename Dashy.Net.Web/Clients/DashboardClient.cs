using Dashy.Net.Web.ViewModels;

namespace Dashy.Net.Web.Clients;

public class DashboardClient(HttpClient httpClient, ILogger<DashboardClient> logger)
{
    public async Task<DashboardConfigVm?> GetDashboardConfigAsync()
    {
        try
        {
            var response = await httpClient.GetFromJsonAsync<DashboardConfigVm>("api/dashboard/config");
            return response;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to fetch dashboard config from the API.");
            return new DashboardConfigVm{Title = "Error: Something went wrong while fetching the dashboard config."};
        }
    }
}

