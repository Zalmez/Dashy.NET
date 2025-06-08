using Dashy.Net.Shared.DTOs;
using Dashy.Net.Shared.ViewModels;

namespace Dashy.Net.Web.Clients;

public class DashboardClient(HttpClient httpClient, ILogger<DashboardClient> logger)
{
    public async Task<DashboardConfigVm?> GetDashboardConfigAsync()
    {
        try
        {
            return await httpClient.GetFromJsonAsync<DashboardConfigVm>("api/dashboard/config");
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to fetch dashboard config from the API.");
            return new DashboardConfigVm("Error: Cannot connect to API!", new List<SectionVm>());
        }
    }

    public async Task<bool> DeleteItemAsync(int itemId)
    {
        try
        {
            var response = await httpClient.DeleteAsync($"api/dashboard/items/{itemId}");
            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Failed to delete item {ItemId}. Status: {StatusCode}", itemId, response.StatusCode);
                return false;
            }
            logger.LogInformation("Successfully deleted item {ItemId}", itemId);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception while deleting item {ItemId}", itemId);
            return false;
        }
    }
    public async Task<bool> UpdateItemAsync(int itemId, UpdateItemDto dto)
    {
        try
        {
            var response = await httpClient.PutAsJsonAsync($"api/dashboard/items/{itemId}", dto);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                logger.LogError("Failed to update item {ItemId}. Status: {StatusCode}, Reason: {Reason}",
                    itemId, response.StatusCode, errorContent);
                return false;
            }
            logger.LogInformation("Successfully updated item {ItemId}", itemId);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception while updating item {ItemId}", itemId);
            return false;
        }
    }

}

