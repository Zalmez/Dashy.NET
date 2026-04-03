using dashy3.Tests.Infrastructure;

namespace dashy3.Tests.Tests;

[Collection("Aspire")]
public class AuthApiTests(AspireFixture fixture)
{
    [Fact]
    public async Task CheckSetup_WhenUsersExist_ReturnsFalse()
    {
        await using var scope = new TestScope(fixture);
        await scope.CreateUserAsync();

        var client = fixture.CreateApiClient();
        var response = await client.GetAsync("/api/auth/check-setup");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<CheckSetupResponse>();
        Assert.False(result?.NeedsSetup);
    }

    [Fact]
    public async Task Register_WithValidData_CreatesUser()
    {
        await using var scope = new TestScope(fixture);
        var client = fixture.CreateApiClient();
        var email = $"test-{Guid.NewGuid():N}@test.local";

        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = "TestPass123!",
            displayName = "Integration Tester",
            role = "User"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(user);
        Assert.Equal(email, user.Email);
        Assert.Equal("Integration Tester", user.DisplayName);
        Assert.Contains("User", user.Roles);
        Assert.NotEmpty(user.Id);

        scope.Track(user.Id); // track for cleanup
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        await using var scope = new TestScope(fixture);
        var (userId, _) = await scope.CreateUserAsync();
        var client = fixture.CreateApiClient();

        // Retrieve email of the created user
        var meReq = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        meReq.Headers.TryAddWithoutValidation("X-User-Id", userId);
        var meRes = await client.SendAsync(meReq);
        var user = await meRes.Content.ReadFromJsonAsync<UserDto>();

        var second = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email = user!.Email, password = "TestPass123!", displayName = "Second", role = "User"
        });

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsUser()
    {
        await using var scope = new TestScope(fixture);
        var email = $"login-{Guid.NewGuid():N}@test.local";
        var (_, _) = await scope.CreateUserAsync(email: email);

        var client = fixture.CreateApiClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email, password = "TestPass123!"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        Assert.Equal(email, user?.Email);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        await using var scope = new TestScope(fixture);
        var email = $"badpw-{Guid.NewGuid():N}@test.local";
        await scope.CreateUserAsync(email: email);

        var client = fixture.CreateApiClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email, password = "WrongPassword!"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMe_WithValidUserId_ReturnsUser()
    {
        await using var scope = new TestScope(fixture);
        var (userId, _) = await scope.CreateUserAsync();

        var client = fixture.CreateApiClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        request.Headers.TryAddWithoutValidation("X-User-Id", userId);
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        Assert.Equal(userId, user?.Id);
    }

    [Fact]
    public async Task GetMe_WithoutUserId_ReturnsUnauthorized()
    {
        var client = fixture.CreateApiClient();
        var response = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProfile_ChangesDisplayName()
    {
        await using var scope = new TestScope(fixture);
        var (userId, _) = await scope.CreateUserAsync();
        var newName = $"Updated {Guid.NewGuid().ToString("N")[..6]}";

        var client = fixture.CreateApiClient();
        var request = new HttpRequestMessage(HttpMethod.Put, "/api/auth/profile");
        request.Headers.TryAddWithoutValidation("X-User-Id", userId);
        request.Content = JsonContent.Create(new { displayName = newName, avatarUrl = (string?)null });
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        Assert.Equal(newName, user?.DisplayName);
    }

    [Fact]
    public async Task ChangePassword_WithCorrectCurrentPassword_Succeeds()
    {
        await using var scope = new TestScope(fixture);
        var email = $"chpw-{Guid.NewGuid():N}@test.local";
        var (userId, _) = await scope.CreateUserAsync(email: email);

        var client = fixture.CreateApiClient();
        var response = await client.PostAsJsonAsync("/api/auth/change-password", new
        {
            userId,
            currentPassword = "TestPass123!",
            newPassword = "NewPass456!"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify new password works
        var loginAgain = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email, password = "NewPass456!"
        });
        Assert.Equal(HttpStatusCode.OK, loginAgain.StatusCode);
    }

    [Fact]
    public async Task Register_AsAdmin_HasAdminRole()
    {
        await using var scope = new TestScope(fixture);
        var (userId, _) = await scope.CreateUserAsync(role: "Admin");

        var client = fixture.CreateApiClient();
        var req = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        req.Headers.TryAddWithoutValidation("X-User-Id", userId);
        var res = await client.SendAsync(req);
        var user = await res.Content.ReadFromJsonAsync<UserDto>();
        Assert.Contains("Admin", user?.Roles ?? []);
    }

    private record UserDto(string Id, string Email, string DisplayName, string? AvatarUrl, string[] Roles, DateTime CreatedAt);
    private record CheckSetupResponse(bool NeedsSetup);
}
