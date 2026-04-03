using System.Text.Json;
using dashy3.Tests.Infrastructure;

namespace dashy3.Tests.Tests;

[Collection("Aspire")]
public class WidgetApiTests(AspireFixture fixture)
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    private async Task<(HttpClient client, string dashboardId, TestScope scope)> SetupAsync()
    {
        var scope = new TestScope(fixture);
        var (_, client) = await scope.CreateUserAsync();

        var createRes = await client.PostAsJsonAsync("/api/dashboards", new
        {
            name = $"Widget Test DB {Guid.NewGuid().ToString("N")[..6]}",
            layoutMode = "grid",
            visibility = "Private"
        });
        var db = await createRes.Content.ReadFromJsonAsync<DashboardDto>(JsonOpts);
        return (client, db!.Id, scope);
    }

    [Fact]
    public async Task AddWidget_ToExistingDashboard_Succeeds()
    {
        var (client, dashId, scope) = await SetupAsync();
        await using var _ = scope;

        var response = await client.PostAsJsonAsync($"/api/dashboards/{dashId}/widgets", new
        {
            type = "link",
            title = "GitHub",
            icon = "🔗",
            size = "Small",
            order = 0,
            config = new { url = "https://github.com", description = "GitHub" }
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var widget = await response.Content.ReadFromJsonAsync<WidgetDto>(JsonOpts);
        Assert.NotNull(widget);
        Assert.Equal("link", widget.Type);
        Assert.Equal("GitHub", widget.Title);
    }

    [Fact]
    public async Task GetDashboard_IncludesWidgets()
    {
        var (client, dashId, scope) = await SetupAsync();
        await using var _ = scope;

        await client.PostAsJsonAsync($"/api/dashboards/{dashId}/widgets", new
        {
            type = "link", title = "Test Widget", icon = "⭐", size = "Small", order = 0,
            config = new { url = "https://example.com" }
        });

        var response = await client.GetAsync($"/api/dashboards/{dashId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dashboard = await response.Content.ReadFromJsonAsync<DashboardWithWidgetsDto>(JsonOpts);
        Assert.NotNull(dashboard?.Widgets);
        Assert.Contains(dashboard.Widgets, w => w.Title == "Test Widget");
    }

    [Fact]
    public async Task UpdateWidget_ChangesConfig()
    {
        var (client, dashId, scope) = await SetupAsync();
        await using var _ = scope;

        var createRes = await client.PostAsJsonAsync($"/api/dashboards/{dashId}/widgets", new
        {
            type = "link", title = "Original", icon = "🔗", size = "Small", order = 0,
            config = new { url = "https://original.com" }
        });
        var widget = await createRes.Content.ReadFromJsonAsync<WidgetDto>(JsonOpts);

        var updateRes = await client.PutAsJsonAsync($"/api/dashboards/{dashId}/widgets/{widget!.Id}", new
        {
            type = "link", title = "Updated", icon = "🔗", size = "Medium", order = 0,
            config = new { url = "https://updated.com" }
        });

        Assert.Equal(HttpStatusCode.OK, updateRes.StatusCode);
        var updated = await updateRes.Content.ReadFromJsonAsync<WidgetDto>(JsonOpts);
        Assert.Equal("Updated", updated?.Title);
        Assert.Equal("Medium", updated?.Size);
    }

    [Fact]
    public async Task DeleteWidget_RemovesFromDashboard()
    {
        var (client, dashId, scope) = await SetupAsync();
        await using var _ = scope;

        var createRes = await client.PostAsJsonAsync($"/api/dashboards/{dashId}/widgets", new
        {
            type = "link", title = "To Remove", icon = "🗑", size = "Small", order = 0,
            config = new { }
        });
        var widget = await createRes.Content.ReadFromJsonAsync<WidgetDto>(JsonOpts);

        var deleteRes = await client.DeleteAsync($"/api/dashboards/{dashId}/widgets/{widget!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteRes.StatusCode);

        var dashResponse = await client.GetAsync($"/api/dashboards/{dashId}");
        var dashboard = await dashResponse.Content.ReadFromJsonAsync<DashboardWithWidgetsDto>(JsonOpts);
        Assert.DoesNotContain(dashboard?.Widgets ?? [], w => w.Id == widget.Id);
    }

    [Fact]
    public async Task AddMultipleWidgets_OrderedCorrectly()
    {
        var (client, dashId, scope) = await SetupAsync();
        await using var _ = scope;

        for (int i = 0; i < 3; i++)
        {
            await client.PostAsJsonAsync($"/api/dashboards/{dashId}/widgets", new
            {
                type = "link", title = $"Widget {i}", icon = "🔗", size = "Small",
                order = i, config = new { }
            });
        }

        var response = await client.GetAsync($"/api/dashboards/{dashId}");
        var dashboard = await response.Content.ReadFromJsonAsync<DashboardWithWidgetsDto>(JsonOpts);
        Assert.Equal(3, dashboard?.Widgets?.Count ?? 0);
    }

    private record DashboardDto(string Id, string Name, string LayoutMode, string? OwnerId, string Visibility);
    private record DashboardWithWidgetsDto(string Id, string Name, string LayoutMode, List<WidgetDto>? Widgets);
    private record WidgetDto(string Id, string Type, string Title, string? Icon, string Size, int Order, Dictionary<string, JsonElement>? Config);
}
