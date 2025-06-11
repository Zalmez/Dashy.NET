using Dashy.Net.Shared.Models;
using System.Net.Http.Json;

namespace Dashy.Net.Web.Clients;

public class HeaderButtonsClient(HttpClient httpClient, ILogger<HeaderButtonsClient> logger)
{
    // TODO: Add error handling and logging

    public async Task<bool> CreateAsync(CreateHeaderButtonDto dto)
    {
        var response = await httpClient.PostAsJsonAsync("api/header-buttons", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateAsync(int buttonId, UpdateHeaderButtonDto dto)
    {
        var response = await httpClient.PutAsJsonAsync($"api/header-buttons/{buttonId}", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(int buttonId)
    {
        var response = await httpClient.DeleteAsync($"api/header-buttons/{buttonId}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ReorderAsync(ReorderHeaderButtonsDto dto)
    {
        var response = await httpClient.PostAsJsonAsync("api/header-buttons/reorder", dto);
        return response.IsSuccessStatusCode;
    }
}