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

    public DashboardClient(HttpClient httpClient, ILoggerFactory loggerFactory)
    {
        _httpClient = httpClient;
        _logger = loggerFactory.CreateLogger<DashboardClient>();

        Items = new ItemsClient(httpClient, loggerFactory.CreateLogger<ItemsClient>());
        Sections = new SectionsClient(httpClient, loggerFactory.CreateLogger<SectionsClient>());
        HeaderButtons = new HeaderButtonsClient(httpClient, loggerFactory.CreateLogger<HeaderButtonsClient>());
    }
    public async Task<DashboardConfigVm?> GetConfigAsync()
    {
        int retryCount = 0;
        const int maxRetries = 3;
        bool success = false;
        while (retryCount < maxRetries && !success)
        {
            try
            {
                retryCount++;
                var response = await _httpClient.GetFromJsonAsync<DashboardConfigVm>("api/dashboard/config");
                success = response is not null && response.Id >= 0;
                return response;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to fetch dashboard config from the API.");
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
        return new DashboardConfigVm
        (
            Id: 0,
            Title: "Error: Cannot connect to API! (Unknown error)",
            Subtitle: null,
            Sections: new(),
            HeaderButtons: new()
        );
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