using System.Text.Json;
using dashy3.Tests.Infrastructure;

namespace dashy3.Tests.Tests;

[Collection("Aspire")]
public class AdminApiTests(AspireFixture fixture)
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public async Task GetUsers_ReturnsUserList()
    {
        await using var scope = new TestScope(fixture);
        var (_, adminClient) = await scope.CreateUserAsync(role: "Admin");
        await scope.CreateUserAsync(); // extra user to ensure list is non-empty

        var response = await adminClient.GetAsync("/api/admin/users");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var users = await response.Content.ReadFromJsonAsync<List<UserDto>>(JsonOpts);
        Assert.NotNull(users);
        Assert.NotEmpty(users);
    }

    [Fact]
    public async Task GetUsers_IncludesRoles()
    {
        await using var scope = new TestScope(fixture);
        var (adminId, adminClient) = await scope.CreateUserAsync(role: "Admin");

        var response = await adminClient.GetAsync("/api/admin/users");
        var users = await response.Content.ReadFromJsonAsync<List<UserDto>>(JsonOpts);

        var admin = users?.FirstOrDefault(u => u.Id == adminId);
        Assert.NotNull(admin);
        Assert.Contains("Admin", admin.Roles);
    }

    [Fact]
    public async Task AddAdminRole_GrantsAdminToUser()
    {
        await using var scope = new TestScope(fixture);
        var (_, adminClient) = await scope.CreateUserAsync(role: "Admin");
        var (userId, _) = await scope.CreateUserAsync(role: "User");

        var response = await adminClient.PutAsJsonAsync($"/api/admin/users/{userId}/role", new
        {
            role = "Admin",
            action = "add"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<UserDto>(JsonOpts);
        Assert.Contains("Admin", user?.Roles ?? []);
    }

    [Fact]
    public async Task RemoveAdminRole_RevokesAdminFromUser()
    {
        await using var scope = new TestScope(fixture);
        var (_, adminClient) = await scope.CreateUserAsync(role: "Admin");
        var (secondAdminId, _) = await scope.CreateUserAsync(role: "Admin");

        var response = await adminClient.PutAsJsonAsync($"/api/admin/users/{secondAdminId}/role", new
        {
            role = "Admin",
            action = "remove"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<UserDto>(JsonOpts);
        Assert.DoesNotContain("Admin", user?.Roles ?? []);
    }

    [Fact]
    public async Task UpdateRole_WithInvalidAction_ReturnsBadRequest()
    {
        await using var scope = new TestScope(fixture);
        var (_, adminClient) = await scope.CreateUserAsync(role: "Admin");
        var (userId, _) = await scope.CreateUserAsync();

        var response = await adminClient.PutAsJsonAsync($"/api/admin/users/{userId}/role", new
        {
            role = "Admin",
            action = "invalid-action"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_RemovesUserFromList()
    {
        await using var scope = new TestScope(fixture);
        var (_, adminClient) = await scope.CreateUserAsync(role: "Admin");
        var (userId, _) = await scope.CreateUserAsync();

        var deleteRes = await adminClient.DeleteAsync($"/api/admin/users/{userId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteRes.StatusCode);

        // Verify user is gone
        var usersRes = await adminClient.GetAsync("/api/admin/users");
        var users = await usersRes.Content.ReadFromJsonAsync<List<UserDto>>(JsonOpts);
        Assert.DoesNotContain(users ?? [], u => u.Id == userId);

        // Already deleted — remove from scope tracking so cleanup doesn't 404 noisily
        // (cleanup will attempt delete and gracefully ignore 404)
    }

    [Fact]
    public async Task DeleteUser_CannotDeleteSelf()
    {
        await using var scope = new TestScope(fixture);
        var (adminId, adminClient) = await scope.CreateUserAsync(role: "Admin");

        var response = await adminClient.DeleteAsync($"/api/admin/users/{adminId}");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_NonExistentUser_ReturnsNotFound()
    {
        await using var scope = new TestScope(fixture);
        var (_, adminClient) = await scope.CreateUserAsync(role: "Admin");

        var fakeId = Guid.NewGuid().ToString();
        var response = await adminClient.DeleteAsync($"/api/admin/users/{fakeId}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private record UserDto(string Id, string Email, string DisplayName, string? AvatarUrl, string[] Roles, DateTime CreatedAt);
}
