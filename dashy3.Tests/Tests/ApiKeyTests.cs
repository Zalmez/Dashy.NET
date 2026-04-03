using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using dashy3.Tests.Infrastructure;

namespace dashy3.Tests.Tests;

[Collection("Aspire")]
public class ApiKeyTests(AspireFixture fixture)
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Creates an API key for the given user. Merges optional extra fields (isGlobal,
    /// allowedDashboardIds, expiresAt) into the request body alongside the key name.
    /// </summary>
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

    /// <summary>Creates an HttpClient that authenticates using an API key header.</summary>
    private HttpClient CreateApiKeyClient(string rawKey)
    {
        var client = fixture.CreateApiClient();
        client.DefaultRequestHeaders.TryAddWithoutValidation("X-Api-Key", rawKey);
        return client;
    }

    [Fact]
    public async Task CreateApiKey_ReturnsKeyWithPrefix()
    {
        await using var scope = new TestScope(fixture);
        var (_, client) = await scope.CreateUserAsync();

        var response = await client.PostAsJsonAsync("/api/keys", new { name = "My Key" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var key = await response.Content.ReadFromJsonAsync<ApiKeyCreatedDto>(JsonOpts);
        Assert.NotNull(key);
        Assert.False(string.IsNullOrEmpty(key.Id));
        Assert.False(string.IsNullOrEmpty(key.Key));
        Assert.True(key.Key.StartsWith("dk_"), $"Key should start with 'dk_', got: {key.Key}");
        Assert.False(string.IsNullOrEmpty(key.Prefix));
        Assert.True(key.Key.StartsWith(key.Prefix), $"Key '{key.Key}' should start with prefix '{key.Prefix}'");
        Assert.Equal("My Key", key.Name);
    }

    [Fact]
    public async Task CreateApiKey_KeyNotReturnedOnList()
    {
        await using var scope = new TestScope(fixture);
        var (_, client) = await scope.CreateUserAsync();

        var created = await CreateApiKeyAsync(client, "List Test Key");

        var listResponse = await client.GetAsync("/api/keys");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        // Raw key must NOT appear anywhere in the list response body
        var listContent = await listResponse.Content.ReadAsStringAsync();
        Assert.DoesNotContain(created.Key, listContent);

        // List items should have prefix but no raw key field
        var keys = await JsonSerializer.DeserializeAsync<List<ApiKeyDto>>(
            await listResponse.Content.ReadAsStreamAsync(), JsonOpts);
        var listed = Assert.Single(keys ?? [], k => k.Id == created.Id);
        Assert.Equal(created.Prefix, listed.Prefix);
    }

    [Fact]
    public async Task ListApiKeys_ReturnsOwnKeysOnly()
    {
        await using var scope = new TestScope(fixture);
        var (_, client1) = await scope.CreateUserAsync();
        var (_, client2) = await scope.CreateUserAsync();

        var key1 = await CreateApiKeyAsync(client1, "User1 Key");
        var key2 = await CreateApiKeyAsync(client2, "User2 Key");

        var list1 = await (await client1.GetAsync("/api/keys"))
            .Content.ReadFromJsonAsync<List<ApiKeyDto>>(JsonOpts);
        Assert.Contains(list1 ?? [], k => k.Id == key1.Id);
        Assert.DoesNotContain(list1 ?? [], k => k.Id == key2.Id);

        var list2 = await (await client2.GetAsync("/api/keys"))
            .Content.ReadFromJsonAsync<List<ApiKeyDto>>(JsonOpts);
        Assert.Contains(list2 ?? [], k => k.Id == key2.Id);
        Assert.DoesNotContain(list2 ?? [], k => k.Id == key1.Id);
    }

    [Fact]
    public async Task DeleteApiKey_RevokesKey()
    {
        await using var scope = new TestScope(fixture);
        var (_, client) = await scope.CreateUserAsync();

        var created = await CreateApiKeyAsync(client, "Revoke Test Key");

        var deleteResponse = await client.DeleteAsync($"/api/keys/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        using var apiKeyClient = CreateApiKeyClient(created.Key);
        var response = await apiKeyClient.GetAsync("/api/dashboards");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteApiKey_OtherUserCannot()
    {
        await using var scope = new TestScope(fixture);
        var (_, clientA) = await scope.CreateUserAsync();
        var (_, clientB) = await scope.CreateUserAsync();

        var key = await CreateApiKeyAsync(clientA, "Protected Key");

        var deleteResponse = await clientB.DeleteAsync($"/api/keys/{key.Id}");
        Assert.Equal(HttpStatusCode.Forbidden, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteApiKey_NotFound_Returns404()
    {
        await using var scope = new TestScope(fixture);
        var (_, client) = await scope.CreateUserAsync();

        var fakeId = Guid.NewGuid().ToString();
        var response = await client.DeleteAsync($"/api/keys/{fakeId}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ApiKey_CanAccessOwnDashboard()
    {
        await using var scope = new TestScope(fixture);
        var (_, client) = await scope.CreateUserAsync();

        var dashRes = await client.PostAsJsonAsync("/api/dashboards", new
        {
            name = "ApiKey Dashboard Test", layoutMode = "grid", visibility = "Private"
        });
        var dashboard = await dashRes.Content.ReadFromJsonAsync<DashboardDto>(JsonOpts);

        var created = await CreateApiKeyAsync(client, "Access Test Key");

        using var apiKeyClient = CreateApiKeyClient(created.Key);
        var response = await apiKeyClient.GetAsync("/api/dashboards");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dashboards = await response.Content.ReadFromJsonAsync<List<DashboardDto>>(JsonOpts);
        Assert.Contains(dashboards ?? [], d => d.Id == dashboard!.Id);
    }

    [Fact]
    public async Task ApiKey_CannotAccessPrivateDashboardOfOtherUser()
    {
        await using var scope = new TestScope(fixture);
        var (_, client1) = await scope.CreateUserAsync();
        var (_, client2) = await scope.CreateUserAsync();

        var dashRes = await client1.PostAsJsonAsync("/api/dashboards", new
        {
            name = "User1 Private Dashboard", layoutMode = "grid", visibility = "Private"
        });
        var dashboard = await dashRes.Content.ReadFromJsonAsync<DashboardDto>(JsonOpts);

        var created = await CreateApiKeyAsync(client2, "User2 Key");

        using var apiKeyClient = CreateApiKeyClient(created.Key);
        var response = await apiKeyClient.GetAsync("/api/dashboards");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dashboards = await response.Content.ReadFromJsonAsync<List<DashboardDto>>(JsonOpts);
        Assert.DoesNotContain(dashboards ?? [], d => d.Id == dashboard!.Id);
    }

    [Fact]
    public async Task ApiKey_Expired_Returns401()
    {
        await using var scope = new TestScope(fixture);
        var (_, client) = await scope.CreateUserAsync();

        var response = await client.PostAsJsonAsync("/api/keys", new
        {
            name = "Expired Key",
            expiresAt = DateTime.UtcNow.AddMinutes(-1).ToString("o")
        });

        if (response.StatusCode == HttpStatusCode.Created)
        {
            var created = await response.Content.ReadFromJsonAsync<ApiKeyCreatedDto>(JsonOpts);
            using var apiKeyClient = CreateApiKeyClient(created!.Key);
            var useResponse = await apiKeyClient.GetAsync("/api/dashboards");
            Assert.Equal(HttpStatusCode.Unauthorized, useResponse.StatusCode);
        }
        else
        {
            Assert.True(
                response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.UnprocessableEntity,
                $"Expected 201, 400, or 422 for past-expiry key creation, got {(int)response.StatusCode}");
        }
    }

    [Fact]
    public async Task AdminKey_Global_CanAccessAllDashboards()
    {
        await using var scope = new TestScope(fixture);
        var (_, adminClient) = await scope.CreateUserAsync(role: "Admin");
        var (_, userClient) = await scope.CreateUserAsync();

        // Regular user creates a private dashboard
        var dashRes = await userClient.PostAsJsonAsync("/api/dashboards", new
        {
            name = "User Private Dashboard", layoutMode = "grid", visibility = "Private"
        });
        var dashboard = await dashRes.Content.ReadFromJsonAsync<DashboardDto>(JsonOpts);

        // Admin creates a global key
        var created = await CreateApiKeyAsync(adminClient, "Admin Global Key", new { isGlobal = true });

        using var apiKeyClient = CreateApiKeyClient(created.Key);
        var response = await apiKeyClient.GetAsync("/api/dashboards");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dashboards = await response.Content.ReadFromJsonAsync<List<DashboardDto>>(JsonOpts);
        Assert.Contains(dashboards ?? [], d => d.Id == dashboard!.Id);
    }

    [Fact]
    public async Task AdminKey_DashboardScoped_CanAccessAllowedDashboard()
    {
        await using var scope = new TestScope(fixture);
        var (_, adminClient) = await scope.CreateUserAsync(role: "Admin");

        var dashRes = await adminClient.PostAsJsonAsync("/api/dashboards", new
        {
            name = "Allowed Dashboard", layoutMode = "grid", visibility = "Private"
        });
        var dashboard = await dashRes.Content.ReadFromJsonAsync<DashboardDto>(JsonOpts);

        var created = await CreateApiKeyAsync(adminClient, "Scoped Key",
            new { allowedDashboardIds = new[] { dashboard!.Id } });

        using var apiKeyClient = CreateApiKeyClient(created.Key);
        var response = await apiKeyClient.GetAsync("/api/dashboards");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dashboards = await response.Content.ReadFromJsonAsync<List<DashboardDto>>(JsonOpts);
        Assert.Contains(dashboards ?? [], d => d.Id == dashboard.Id);
    }

    [Fact]
    public async Task AdminKey_DashboardScoped_CannotAccessOtherDashboard()
    {
        await using var scope = new TestScope(fixture);
        var (_, adminClient) = await scope.CreateUserAsync(role: "Admin");
        var (_, userClient) = await scope.CreateUserAsync();

        var allowedDashRes = await adminClient.PostAsJsonAsync("/api/dashboards", new
        {
            name = "Allowed Dashboard", layoutMode = "grid", visibility = "Private"
        });
        var allowedDash = await allowedDashRes.Content.ReadFromJsonAsync<DashboardDto>(JsonOpts);

        var otherDashRes = await userClient.PostAsJsonAsync("/api/dashboards", new
        {
            name = "Other User Private Dashboard", layoutMode = "grid", visibility = "Private"
        });
        var otherDash = await otherDashRes.Content.ReadFromJsonAsync<DashboardDto>(JsonOpts);

        // Scoped to only the allowed dashboard
        var created = await CreateApiKeyAsync(adminClient, "Limited Scoped Key",
            new { allowedDashboardIds = new[] { allowedDash!.Id } });

        using var apiKeyClient = CreateApiKeyClient(created.Key);
        var response = await apiKeyClient.GetAsync("/api/dashboards");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dashboards = await response.Content.ReadFromJsonAsync<List<DashboardDto>>(JsonOpts);

        Assert.Contains(dashboards ?? [], d => d.Id == allowedDash.Id);
        Assert.DoesNotContain(dashboards ?? [], d => d.Id == otherDash!.Id);
    }

    [Fact]
    public async Task NonAdmin_CannotCreateGlobalKey()
    {
        await using var scope = new TestScope(fixture);
        var (_, client) = await scope.CreateUserAsync();

        var response = await client.PostAsJsonAsync("/api/keys", new
        {
            name = "Unauthorized Global Key",
            isGlobal = true
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

    private record ApiKeyDto(
        string Id,
        string Name,
        string Prefix,
        DateTime CreatedAt,
        DateTime? ExpiresAt,
        DateTime? LastUsedAt,
        bool IsGlobal,
        List<string>? AllowedDashboardIds);

    private record DashboardDto(string Id, string Name, string LayoutMode, string? OwnerId, string Visibility);
}
