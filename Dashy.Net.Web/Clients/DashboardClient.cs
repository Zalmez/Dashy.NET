using Dashy.Net.Shared.ViewModels;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dashy.Net.Web.Clients;

public class DashboardClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DashboardClient> _logger;

    public ItemsClient Items { get; }
    public SectionsClient Sections { get; }

    public DashboardClient(HttpClient httpClient, ILoggerFactory loggerFactory)
    {
        _httpClient = httpClient;
        _logger = loggerFactory.CreateLogger<DashboardClient>();

        Items = new ItemsClient(httpClient, loggerFactory.CreateLogger<ItemsClient>());
        Sections = new SectionsClient(httpClient, loggerFactory.CreateLogger<SectionsClient>());
    }
    public async Task<DashboardConfigVm?> GetConfigAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<DashboardConfigVm>("api/dashboard/config");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch dashboard config from the API.");
            return new DashboardConfigVm
            (
                Id: 0,
                Title: "Error: Cannot connect to API!",
                Subtitle: null,
                Sections: new List<SectionVm>()
            );
        }
    }
}