using System.Text.Json;
using dashy3.Tests.Infrastructure;

namespace dashy3.Tests.Tests;

[Collection("Aspire")]
public class DashboardApiTests(AspireFixture fixture)
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public async Task CreateDashboard_ReturnsCreatedDashboard()
    {
        await using var scope = new TestScope(fixture);
        var (_, client) = await scope.CreateUserAsync();

        var response = await client.PostAsJsonAsync("/api/dashboards", new
        {
            name = "My Test Dashboard",
            layoutMode = "grid",
            visibility = "Private"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var dashboard = await response.Content.ReadFromJsonAsync<DashboardDto>(JsonOpts);
        Assert.NotNull(dashboard);
        Assert.Equal("My Test Dashboard", dashboard.Name);
        Assert.Equal("Private", dashboard.Visibility);
        Assert.False(string.IsNullOrEmpty(dashboard.Id));
    }

    [Fact]
    public async Task GetDashboards_ReturnsDashboardsForUser()
    {
        await using var scope = new TestScope(fixture);
        var (_, client) = await scope.CreateUserAsync();
        var name = $"Dashboard {Guid.NewGuid().ToString("N")[..8]}";

        await client.PostAsJsonAsync("/api/dashboards", new
        {
            name, layoutMode = "grid", visibility = "Private"
        });

        var response = await client.GetAsync("/api/dashboards");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dashboards = await response.Content.ReadFromJsonAsync<List<DashboardDto>>(JsonOpts);
        Assert.NotNull(dashboards);
        Assert.Contains(dashboards, d => d.Name == name);
    }

    [Fact]
    public async Task GetDashboards_DoesNotReturnOtherUserPrivateDashboards()
    {
        await using var scope = new TestScope(fixture);
        var (_, client1) = await scope.CreateUserAsync();
        var (_, client2) = await scope.CreateUserAsync();

        var name = $"Private {Guid.NewGuid().ToString("N")[..8]}";
        await client1.PostAsJsonAsync("/api/dashboards", new
        {
            name, layoutMode = "grid", visibility = "Private"
        });

        var response = await client2.GetAsync("/api/dashboards");
        var dashboards = await response.Content.ReadFromJsonAsync<List<DashboardDto>>(JsonOpts);
        Assert.DoesNotContain(dashboards ?? [], d => d.Name == name);
    }

    [Fact]
    public async Task GetDashboards_ReturnsSharedDashboardsFromOtherUsers()
    {
        await using var scope = new TestScope(fixture);
        var (_, client1) = await scope.CreateUserAsync();
        var (_, client2) = await scope.CreateUserAsync();

        var createRes = await client1.PostAsJsonAsync("/api/dashboards", new
        {
            name = $"Shared {Guid.NewGuid().ToString("N")[..8]}",
            layoutMode = "grid",
            visibility = "Shared"
        });
        var dashboard = await createRes.Content.ReadFromJsonAsync<DashboardDto>(JsonOpts);

        var response = await client2.GetAsync("/api/dashboards");
        var dashboards = await response.Content.ReadFromJsonAsync<List<DashboardDto>>(JsonOpts);
        Assert.Contains(dashboards ?? [], d => d.Id == dashboard!.Id);
    }

    [Fact]
    public async Task UpdateDashboard_ChangesName()
    {
        await using var scope = new TestScope(fixture);
        var (_, client) = await scope.CreateUserAsync();

        var createRes = await client.PostAsJsonAsync("/api/dashboards", new
        {
            name = "Original Name", layoutMode = "grid", visibility = "Private"
        });
        var dashboard = await createRes.Content.ReadFromJsonAsync<DashboardDto>(JsonOpts);

        var updateRes = await client.PutAsJsonAsync($"/api/dashboards/{dashboard!.Id}", new
        {
            name = "Updated Name", layoutMode = "grid"
        });

        Assert.Equal(HttpStatusCode.OK, updateRes.StatusCode);
        var updated = await updateRes.Content.ReadFromJsonAsync<DashboardDto>(JsonOpts);
        Assert.Equal("Updated Name", updated?.Name);
    }

    [Fact]
    public async Task PatchVisibility_TogglesVisibility()
    {
        await using var scope = new TestScope(fixture);
        var (_, client) = await scope.CreateUserAsync();

        var createRes = await client.PostAsJsonAsync("/api/dashboards", new
        {
            name = "Visibility Test", layoutMode = "grid", visibility = "Private"
        });
        var dashboard = await createRes.Content.ReadFromJsonAsync<DashboardDto>(JsonOpts);

        var patchReq = new HttpRequestMessage(HttpMethod.Patch, $"/api/dashboards/{dashboard!.Id}/visibility");
        patchReq.Content = JsonContent.Create(new { visibility = "Shared" });
        var patchRes = await client.SendAsync(patchReq);

        Assert.Equal(HttpStatusCode.OK, patchRes.StatusCode);
        var patched = await patchRes.Content.ReadFromJsonAsync<DashboardDto>(JsonOpts);
        Assert.Equal("Shared", patched?.Visibility);
    }

    [Fact]
    public async Task DeleteDashboard_RemovesDashboard()
    {
        await using var scope = new TestScope(fixture);
        var (_, client) = await scope.CreateUserAsync();

        var createRes = await client.PostAsJsonAsync("/api/dashboards", new
        {
            name = "To Delete", layoutMode = "grid", visibility = "Private"
        });
        var dashboard = await createRes.Content.ReadFromJsonAsync<DashboardDto>(JsonOpts);

        var deleteRes = await client.DeleteAsync($"/api/dashboards/{dashboard!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteRes.StatusCode);

        var getRes = await client.GetAsync($"/api/dashboards/{dashboard.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getRes.StatusCode);
    }

    [Fact]
    public async Task DeleteDashboard_OtherUserCannotDelete()
    {
        await using var scope = new TestScope(fixture);
        var (_, client1) = await scope.CreateUserAsync();
        var (_, client2) = await scope.CreateUserAsync();

        var createRes = await client1.PostAsJsonAsync("/api/dashboards", new
        {
            name = "Protected", layoutMode = "grid", visibility = "Private"
        });
        var dashboard = await createRes.Content.ReadFromJsonAsync<DashboardDto>(JsonOpts);

        var deleteRes = await client2.DeleteAsync($"/api/dashboards/{dashboard!.Id}");
        Assert.Equal(HttpStatusCode.Forbidden, deleteRes.StatusCode);
    }

    private record DashboardDto(string Id, string Name, string LayoutMode, string? OwnerId, string Visibility);
}
