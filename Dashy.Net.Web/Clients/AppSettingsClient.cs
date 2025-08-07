using System.Net.Http.Json;
using Dashy.Net.Shared.Models;

namespace Dashy.Net.Web.Clients;

public class AppSettingsClient(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<AppSettings?> GetAsync()
    {
        return await _httpClient.GetFromJsonAsync<AppSettings>("api/AppSettings");
    }

    public async Task<bool> SetCustomLogoPathAsync(string logoPath)
    {
        var response = await _httpClient.PostAsJsonAsync("api/AppSettings/logo-path", new { path = logoPath });
        return response.IsSuccessStatusCode;
    }

    public async Task RemoveLogoAsync()
        => await _httpClient.DeleteAsync("api/AppSettings/logo");

    private class LogoUploadResult
    {
        public string? Path { get; set; }
    }
}
