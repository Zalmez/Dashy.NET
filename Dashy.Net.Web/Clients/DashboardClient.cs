using Dashy.Net.Shared.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dashy.Net.Web.Clients;

public class DashboardClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DashboardClient> _logger;

    private readonly Dictionary<int, (string etag, DashboardConfigVm config)> _cache = new();

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

    public async Task<DashboardConfigVm?> GetConfigAsync(int? dashboardId = null, CancellationToken cancellationToken = default)
    {
        int retryCount = 0;
        const int maxRetries = 3;
        bool success = false;
        while (retryCount < maxRetries && !success && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                retryCount++;
                var id = dashboardId ?? 1;
                var url = dashboardId.HasValue ? $"api/dashboard/config?id={dashboardId.Value}" : "api/dashboard/config";
                using var req = new HttpRequestMessage(HttpMethod.Get, url);
                if (_cache.TryGetValue(id, out var cached) && !string.IsNullOrEmpty(cached.etag))
                {
                    req.Headers.IfNoneMatch.Add(new EntityTagHeaderValue(cached.etag));
                }
                using var resp = await _httpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                if (resp.StatusCode == HttpStatusCode.NotModified)
                {
                    _logger.LogDebug("Dashboard {DashboardId} not modified (ETag). Using cached config.", id);
                    return cached.config;
                }
                resp.EnsureSuccessStatusCode();
                var result = await resp.Content.ReadFromJsonAsync<DashboardConfigVm>(cancellationToken: cancellationToken);
                var etag = resp.Headers.ETag?.Tag;
                if (result is not null && !string.IsNullOrEmpty(etag))
                {
                    _cache[id] = (etag!, result);
                }
                success = result is not null && result.Id >= 0;
                return result;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Dashboard config request was cancelled (attempt {Attempt}/{Max})", retryCount, maxRetries);
                return null;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to fetch dashboard config from the API.");
                if (ex.StatusCode.HasValue)
                {
                    switch (ex.StatusCode.Value)
                    {
                        case HttpStatusCode.NotFound:
                            await SeedDashboardAsync(cancellationToken);
                            break;
                        case HttpStatusCode.BadGateway:
                            return new DashboardConfigVm(0, "Error: Cannot connect to API! (Bad Gateway error)", null, new(), new(), false);
                        case HttpStatusCode.ServiceUnavailable:
                            return new DashboardConfigVm(0, "Error: Cannot connect to API! (Service Unavailable)", null, new(), new(), false);
                        case HttpStatusCode.GatewayTimeout:
                            return new DashboardConfigVm(0, "Error: Cannot connect to API! (Gateway Timeout)", null, new(), new(), false);
                        default:
                            break;
                    }
                }
            }
        }
        return new DashboardConfigVm(0, "Error: Cannot connect to API! (Unknown error)", null, new(), new(), false);
    }

    public async Task<bool> SetUseContainerWidgetsAsync(int dashboardId, bool enabled, CancellationToken cancellationToken = default)
    {
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, $"api/dashboard/{dashboardId}/use-container-widgets")
            {
                Content = JsonContent.Create(new { enabled })
            };
            if (_cache.TryGetValue(dashboardId, out var cached) && !string.IsNullOrEmpty(cached.etag))
            {
                req.Headers.IfMatch.Add(new EntityTagHeaderValue(cached.etag));
            }
            using var resp = await _httpClient.SendAsync(req, cancellationToken);
            if (resp.IsSuccessStatusCode)
            {
                var etag = resp.Headers.ETag?.Tag;
                if (!string.IsNullOrEmpty(etag) && _cache.TryGetValue(dashboardId, out var c))
                {
                    _cache[dashboardId] = (etag!, c.config);
                }
                return true;
            }
            return false;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("SetUseContainerWidgets cancelled for dashboard {DashboardId}", dashboardId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set UseContainerWidgets for dashboard {DashboardId}", dashboardId);
            return false;
        }
    }

    public async Task<DashboardConfigVm?> CreateAsync(CreateDashboardDto createDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/dashboard", createDto, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<DashboardConfigVm>(cancellationToken: cancellationToken);
                var etag = response.Headers.ETag?.Tag;
                if (result is not null && !string.IsNullOrEmpty(etag))
                {
                    _cache[result.Id] = (etag!, result);
                }
                _logger.LogInformation("Successfully created new dashboard '{DashboardTitle}'", createDto.Title);
                return result;
            }
            else
            {
                _logger.LogError("Failed to create dashboard '{DashboardTitle}'. Status: {StatusCode}", createDto.Title, response.StatusCode);
                return null;
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Create dashboard cancelled for '{DashboardTitle}'", createDto.Title);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while creating dashboard");
            return null;
        }
    }

    public async Task<bool> UpdateAsync(int id, UpdateDashboardDto updateDto, CancellationToken cancellationToken = default)
    {
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Put, $"api/dashboard/{id}")
            {
                Content = JsonContent.Create(updateDto)
            };
            if (_cache.TryGetValue(id, out var cached) && !string.IsNullOrEmpty(cached.etag))
            {
                req.Headers.IfMatch.Add(new EntityTagHeaderValue(cached.etag));
            }
            using var resp = await _httpClient.SendAsync(req, cancellationToken);
            if (resp.IsSuccessStatusCode)
            {
                var etag = resp.Headers.ETag?.Tag;
                if (!string.IsNullOrEmpty(etag) && _cache.TryGetValue(id, out var c))
                {
                    _cache[id] = (etag!, c.config);
                }
                return true;
            }
            return false;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Update dashboard cancelled for {DashboardId}", id);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update dashboard {DashboardId}", id);
            return false;
        }
    }

    public async Task<IEnumerable<DashboardListItemVm>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<DashboardListItemVm>>("api/dashboard/list", cancellationToken);
            return response ?? Enumerable.Empty<DashboardListItemVm>();
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("GetAll dashboards cancelled");
            return Enumerable.Empty<DashboardListItemVm>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch dashboard list");
            return Enumerable.Empty<DashboardListItemVm>();
        }
    }

    public async Task<bool> SeedDashboardAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsync("api/dashboard/seed", null, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("SeedDashboard cancelled");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to seed dashboard.");
            return false;
        }
    }

    public async Task<(List<ItemVm> rootItems, Dictionary<int, List<ItemVm>> children)?> GetFlattenedItemsAsync(int dashboardId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/dashboard/{dashboardId}/items/flat", cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(json);
            var rootItems = new List<ItemVm>();
            if (doc.RootElement.TryGetProperty("rootItems", out var rootArr))
            {
                foreach (var r in rootArr.EnumerateArray())
                {
                    rootItems.Add(new ItemVm(
                        r.GetProperty("id").GetInt32(),
                        r.GetProperty("title").GetString() ?? "Untitled",
                        r.TryGetProperty("icon", out var ic) ? ic.GetString() : null,
                        r.TryGetProperty("widget", out var wd) ? wd.GetString() : null,
                        r.GetProperty("sectionId").GetInt32(),
                        r.TryGetProperty("options", out var opt) ? opt : (JsonElement?)null,
                        r.TryGetProperty("parentItemId", out var pid) && pid.ValueKind != JsonValueKind.Null ? pid.GetInt32() : (int?)null
                    ));
                }
            }
            var children = new Dictionary<int, List<ItemVm>>();
            if (doc.RootElement.TryGetProperty("children", out var childObj))
            {
                foreach (var kv in childObj.EnumerateObject())
                {
                    if (int.TryParse(kv.Name, out var parentId))
                    {
                        var list = new List<ItemVm>();
                        foreach (var c in kv.Value.EnumerateArray())
                        {
                            list.Add(new ItemVm(
                                c.GetProperty("id").GetInt32(),
                                c.GetProperty("title").GetString() ?? "Untitled",
                                c.TryGetProperty("icon", out var icc) ? icc.GetString() : null,
                                c.TryGetProperty("widget", out var wdc) ? wdc.GetString() : null,
                                c.GetProperty("sectionId").GetInt32(),
                                c.TryGetProperty("options", out var optc) ? optc : (JsonElement?)null,
                                c.TryGetProperty("parentItemId", out var pidc) && pidc.ValueKind != JsonValueKind.Null ? pidc.GetInt32() : (int?)null
                            ));
                        }
                        children[parentId] = list;
                    }
                }
            }
            return (rootItems, children);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("GetFlattenedItems cancelled for {DashboardId}", dashboardId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get flattened items for dashboard {DashboardId}", dashboardId);
            return null;
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

    public async Task<IEnumerable<AuthenticationProviderVm>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var providers = await _httpClient.GetFromJsonAsync<IEnumerable<AuthenticationProviderVm>>("api/auth-providers", cancellationToken);
            return providers ?? Enumerable.Empty<AuthenticationProviderVm>();
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("GetAll authentication providers cancelled");
            return Enumerable.Empty<AuthenticationProviderVm>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get authentication providers");
            return Enumerable.Empty<AuthenticationProviderVm>();
        }
    }

    public async Task<AuthenticationProviderVm?> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<AuthenticationProviderVm>($"api/auth-providers/{id}", cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Get authentication provider cancelled for {Id}", id);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get authentication provider {Id}", id);
            return null;
        }
    }

    public async Task<bool> CreateAsync(CreateAuthenticationProviderDto createDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth-providers", createDto, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Create authentication provider cancelled");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create authentication provider");
            return false;
        }
    }

    public async Task<bool> UpdateAsync(int id, UpdateAuthenticationProviderDto updateDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/auth-providers/{id}", updateDto, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Update authentication provider cancelled for {Id}", id);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update authentication provider {Id}", id);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/auth-providers/{id}", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Delete authentication provider cancelled for {Id}", id);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete authentication provider {Id}", id);
            return false;
        }
    }
}

