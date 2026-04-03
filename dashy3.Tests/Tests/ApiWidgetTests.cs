using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using dashy3.Tests.Infrastructure;

namespace dashy3.Tests.Tests;

[Collection("Aspire")]
public class ApiWidgetTests(AspireFixture fixture)
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private async Task<ApiKeyCreatedDto> CreateApiKeyAsync(
        HttpClient client,
        string name,
        object? extras = null)
    {
        var node = new JsonObject { ["name"] = name };

        if (extras is not null)
        {
            var extrasJson = JsonSerializer.Serialize(extras, JsonOpts);
            var extrasNode = JsonNode.Parse(extrasJson)?.AsObject();
            if (extrasNode is not null)
                foreach (var (key, value) in extrasNode)
                    node[key] = value?.DeepClone();
        }

        var response = await client.PostAsJsonAsync("/api/keys", node, JsonOpts);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ApiKeyCreatedDto>(JsonOpts))!;
    }

    private HttpClient CreateApiKeyClient(string rawKey)
    {
        var client = fixture.CreateApiClient();
        client.DefaultRequestHeaders.TryAddWithoutValidation("X-Api-Key", rawKey);
        return client;
    }

    private async Task<DashboardDto> CreateDashboardAsync(HttpClient client, string name = "Test Dashboard")
    {
        var response = await client.PostAsJsonAsync("/api/dashboards", new
        {
            name,
            layoutMode = "grid",
            visibility = "Private"
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<DashboardDto>(JsonOpts))!;
    }

    private async Task<WidgetDto> AddApiWidgetAsync(HttpClient client, string dashboardId, string title = "My API Widget")
    {
        var response = await client.PostAsJsonAsync($"/api/dashboards/{dashboardId}/widgets", new
        {
            type = "api",
            title,
            icon = "Code",
            size = "Medium"
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<WidgetDto>(JsonOpts))!;
    }

    [Fact]
    public async Task UpdateWidgetContent_WithApiKey_Succeeds()
    {
        await using var scope = new TestScope(fixture);
        var (_, client) = await scope.CreateUserAsync();

        var dashboard = await CreateDashboardAsync(client);
        var widget = await AddApiWidgetAsync(client, dashboard.Id);
        var created = await CreateApiKeyAsync(client, "Content Update Key");

        using var apiKeyClient = CreateApiKeyClient(created.Key);
        var response = await apiKeyClient.PutAsJsonAsync($"/api/widgets/{widget.Id}/content", new
        {
            content = "<h1>Hello</h1>",
            css = "h1 { color: red; }"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<WidgetUpdateResultDto>(JsonOpts);
        Assert.NotNull(result);
        Assert.Equal(widget.Id, result.WidgetId);
        Assert.NotEqual(default, result.UpdatedAt);
    }

    [Fact]
    public async Task UpdateWidgetContent_InvalidApiKey_Returns401()
    {
        await using var scope = new TestScope(fixture);
        var (_, client) = await scope.CreateUserAsync();

        var dashboard = await CreateDashboardAsync(client);
        var widget = await AddApiWidgetAsync(client, dashboard.Id);

        using var fakeKeyClient = CreateApiKeyClient("dk_totally_fake_key_that_does_not_exist");
        var response = await fakeKeyClient.PutAsJsonAsync($"/api/widgets/{widget.Id}/content", new
        {
            content = "<h1>Hello</h1>",
            css = ""
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateWidgetContent_WithUserId_Succeeds()
    {
        await using var scope = new TestScope(fixture);
        var (userId, client) = await scope.CreateUserAsync();

        var dashboard = await CreateDashboardAsync(client);
        var widget = await AddApiWidgetAsync(client, dashboard.Id);

        var response = await client.PutAsJsonAsync($"/api/widgets/{widget.Id}/content", new
        {
            content = "<h1>Hello from UserId</h1>",
            css = "h1 { color: blue; }"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateWidgetContent_OtherUserApiKey_Returns403()
    {
        await using var scope = new TestScope(fixture);
        var (_, clientA) = await scope.CreateUserAsync();
        var (_, clientB) = await scope.CreateUserAsync();

        // User A creates a private dashboard with an api widget
        var dashboard = await CreateDashboardAsync(clientA, "UserA Private Dashboard");
        var widget = await AddApiWidgetAsync(clientA, dashboard.Id);

        // User B creates an API key
        var keyB = await CreateApiKeyAsync(clientB, "UserB Key");

        // User B's key tries to update User A's widget
        using var apiKeyClientB = CreateApiKeyClient(keyB.Key);
        var response = await apiKeyClientB.PutAsJsonAsync($"/api/widgets/{widget.Id}/content", new
        {
            content = "<h1>Unauthorized</h1>",
            css = ""
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateWidgetContent_ContentTooLarge_Returns400()
    {
        await using var scope = new TestScope(fixture);
        var (_, client) = await scope.CreateUserAsync();

        var dashboard = await CreateDashboardAsync(client);
        var widget = await AddApiWidgetAsync(client, dashboard.Id);
        var created = await CreateApiKeyAsync(client, "Large Content Key");

        using var apiKeyClient = CreateApiKeyClient(created.Key);
        var hugeContent = new string('x', 100_001);
        var response = await apiKeyClient.PutAsJsonAsync($"/api/widgets/{widget.Id}/content", new
        {
            content = hugeContent,
            css = ""
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateWidgetContent_WidgetNotFound_Returns404()
    {
        await using var scope = new TestScope(fixture);
        var (_, client) = await scope.CreateUserAsync();

        var created = await CreateApiKeyAsync(client, "NotFound Key");

        using var apiKeyClient = CreateApiKeyClient(created.Key);
        var fakeWidgetId = Guid.NewGuid().ToString();
        var response = await apiKeyClient.PutAsJsonAsync($"/api/widgets/{fakeWidgetId}/content", new
        {
            content = "<h1>Hello</h1>",
            css = ""
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateWidgetContent_GlobalApiKey_CanUpdateAnyWidget()
    {
        await using var scope = new TestScope(fixture);
        var (_, adminClient) = await scope.CreateUserAsync(role: "Admin");
        var (_, userClient) = await scope.CreateUserAsync();

        // Regular user creates a private dashboard with an api widget
        var dashboard = await CreateDashboardAsync(userClient, "User Private Dashboard");
        var widget = await AddApiWidgetAsync(userClient, dashboard.Id);

        // Admin creates a global API key
        var globalKey = await CreateApiKeyAsync(adminClient, "Admin Global Key", new { isGlobal = true });

        using var globalKeyClient = CreateApiKeyClient(globalKey.Key);
        var response = await globalKeyClient.PutAsJsonAsync($"/api/widgets/{widget.Id}/content", new
        {
            content = "<h1>Updated by Admin</h1>",
            css = "h1 { color: green; }"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateWidgetContent_ScopedApiKey_OnlyAllowedDashboard()
    {
        await using var scope = new TestScope(fixture);
        var (_, adminClient) = await scope.CreateUserAsync(role: "Admin");
        var (_, userClient) = await scope.CreateUserAsync();

        // Dashboard A — the one the scoped key is allowed for
        var dashboardA = await CreateDashboardAsync(adminClient, "Dashboard A");

        // Dashboard B (owned by another user) with an api widget
        var dashboardB = await CreateDashboardAsync(userClient, "Dashboard B");
        var widgetB = await AddApiWidgetAsync(userClient, dashboardB.Id);

        // Admin creates a scoped key restricted to dashboardA only
        var scopedKey = await CreateApiKeyAsync(adminClient, "Scoped Key",
            new { allowedDashboardIds = new[] { dashboardA.Id } });

        // Scoped key tries to update a widget in dashboardB
        using var scopedKeyClient = CreateApiKeyClient(scopedKey.Key);
        var response = await scopedKeyClient.PutAsJsonAsync($"/api/widgets/{widgetB.Id}/content", new
        {
            content = "<h1>Scoped Attempt</h1>",
            css = ""
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private record ApiKeyCreatedDto(
        string Id,
        string Name,
        string Key,
        string Prefix,
        DateTime CreatedAt,
        DateTime? ExpiresAt);

    private record DashboardDto(string Id, string Name);

    private record WidgetDto(string Id, string Type, string Title);

    private record WidgetUpdateResultDto(string WidgetId, DateTime UpdatedAt);
}
