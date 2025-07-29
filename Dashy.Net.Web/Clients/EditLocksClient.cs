using System.Net.Http.Json;
using Dashy.Net.Shared.Models;

namespace Dashy.Net.Web.Clients;

public class EditLocksClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EditLocksClient> _logger;

    public EditLocksClient(HttpClient httpClient, ILogger<EditLocksClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<EditLockDto?> GetEditLockAsync(int dashboardId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<EditLockDto?>($"api/editlocks/{dashboardId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get edit lock for dashboard {DashboardId}", dashboardId);
            return null;
        }
    }

    public async Task<EditLockResponse> AcquireEditLockAsync(int dashboardId, bool forceAcquire = false)
    {
        try
        {
            var request = new AcquireEditLockDto
            {
                DashboardId = dashboardId,
                ForceAcquire = forceAcquire
            };

            var response = await _httpClient.PostAsJsonAsync("api/editlocks/acquire", request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<EditLockResponse>() ??
                       new EditLockResponse { Success = false, ErrorMessage = "Failed to parse response" };
            }

            return new EditLockResponse
            {
                Success = false,
                ErrorMessage = $"API call failed with status {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to acquire edit lock for dashboard {DashboardId}", dashboardId);
            return new EditLockResponse
            {
                Success = false,
                ErrorMessage = "Network error occurred"
            };
        }
    }

    public async Task<bool> ReleaseEditLockAsync(int dashboardId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"api/editlocks/{dashboardId}/release", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to release edit lock for dashboard {DashboardId}", dashboardId);
            return false;
        }
    }

    public async Task<bool> UpdateActivityAsync(int dashboardId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"api/editlocks/{dashboardId}/activity", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update activity for dashboard {DashboardId}", dashboardId);
            return false;
        }
    }
}
