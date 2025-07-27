using Dashy.Net.Shared.Models;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dashy.Net.Web.Clients;

public class DashboardClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DashboardClient> _logger;

    public ItemsClient Items { get; }
    public SectionsClient Sections { get; }
    public HeaderButtonsClient HeaderButtons { get; }
    public SettingsClient Settings { get; }

    public DashboardClient(HttpClient httpClient, ILoggerFactory loggerFactory)
    {
        _httpClient = httpClient;
        _logger = loggerFactory.CreateLogger<DashboardClient>();

        Items = new ItemsClient(httpClient, loggerFactory.CreateLogger<ItemsClient>());
        Sections = new SectionsClient(httpClient, loggerFactory.CreateLogger<SectionsClient>());
        HeaderButtons = new HeaderButtonsClient(httpClient, loggerFactory.CreateLogger<HeaderButtonsClient>());
        Settings = new SettingsClient(httpClient, loggerFactory);
    }
    public async Task<DashboardConfigVm?> GetConfigAsync(int? dashboardId = null)
    {
        int retryCount = 0;
        const int maxRetries = 3;
        bool success = false;
        while (retryCount < maxRetries && !success)
        {
            try
            {
                retryCount++;
                var url = dashboardId.HasValue ? $"api/dashboard/config?id={dashboardId.Value}" : "api/dashboard/config";
                var response = await _httpClient.GetFromJsonAsync<DashboardConfigVm>(url);
                success = response is not null && response.Id >= 0;
                return response;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to fetch dashboard config from the API.");
                if (ex.StatusCode.HasValue)
                {
                    switch (ex.StatusCode.Value)
                    {
                        case HttpStatusCode.NotFound:
                            await SeedDashboardAsync();
                            break;
                        case HttpStatusCode.BadGateway:
                            return new DashboardConfigVm
                            (
                                Id: 0,
                                Title: "Error: Cannot connect to API! (Bad Gateway error)",
                                Subtitle: null,
                                Sections: new(),
                                HeaderButtons: new()
                            );
                        case HttpStatusCode.ServiceUnavailable:
                            return new DashboardConfigVm
                            (
                                Id: 0,
                                Title: "Error: Cannot connect to API! (Service Unavailable)",
                                Subtitle: null,
                                Sections: new(),
                                HeaderButtons: new()
                            );
                        case HttpStatusCode.GatewayTimeout:
                            return new DashboardConfigVm
                            (
                                Id: 0,
                                Title: "Error: Cannot connect to API! (Gateway Timeout)",
                                Subtitle: null,
                                Sections: new(),
                                HeaderButtons: new()
                            );
                        default:
                            break;
                    }
                }

            }
        }
        return new DashboardConfigVm
        (
            Id: 0,
            Title: "Error: Cannot connect to API! (Unknown error)",
            Subtitle: null,
            Sections: new(),
            HeaderButtons: new()
        );
    }

    public async Task<DashboardConfigVm?> CreateAsync(CreateDashboardDto createDto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/dashboard", createDto);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<DashboardConfigVm>();
                _logger.LogInformation("Successfully created new dashboard '{DashboardTitle}'", createDto.Title);
                return result;
            }
            else
            {
                _logger.LogError("Failed to create dashboard '{DashboardTitle}'. Status: {StatusCode}", createDto.Title, response.StatusCode);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while creating dashboard");
            return null;
        }
    }

    public async Task<bool> UpdateAsync(int id, UpdateDashboardDto updateDto)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/dashboard/{id}", updateDto);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update dashboard {DashboardId}", id);
            return false;
        }
    }

    public async Task<IEnumerable<DashboardListItemVm>> GetAllAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<DashboardListItemVm>>("api/dashboard/list");
            return response ?? Enumerable.Empty<DashboardListItemVm>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch dashboard list");
            return Enumerable.Empty<DashboardListItemVm>();
        }
    }

    public async Task<bool> SeedDashboardAsync()
    {
        try
        {
            var response = await _httpClient.PostAsync("api/dashboard/seed", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to seed dashboard.");
            return false;
        }
    }
}



public class ProvidersClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProvidersClient> _logger;

    public ProvidersClient(HttpClient httpClient, ILogger<ProvidersClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IEnumerable<AuthenticationProviderVm>> GetAllAsync()
    {
        try
        {
            var providers = await _httpClient.GetFromJsonAsync<IEnumerable<AuthenticationProviderVm>>("api/auth-providers");
            return providers ?? Enumerable.Empty<AuthenticationProviderVm>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get authentication providers");
            return Enumerable.Empty<AuthenticationProviderVm>();
        }
    }

    public async Task<AuthenticationProviderVm?> GetAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<AuthenticationProviderVm>($"api/auth-providers/{id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get authentication provider {Id}", id);
            return null;
        }
    }

    public async Task<bool> CreateAsync(CreateAuthenticationProviderDto createDto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth-providers", createDto);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create authentication provider");
            return false;
        }
    }

    public async Task<bool> UpdateAsync(int id, UpdateAuthenticationProviderDto updateDto)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/auth-providers/{id}", updateDto);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update authentication provider {Id}", id);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/auth-providers/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete authentication provider {Id}", id);
            return false;
        }
    }
}

