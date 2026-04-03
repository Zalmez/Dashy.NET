using System.Text.Json;
using System.Text.Json.Serialization;

namespace dashy3.Tests.Infrastructure;

/// <summary>
/// Helper for registering test users and setting up authenticated HTTP clients.
/// </summary>
public static class TestAuthHelper
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Registers a unique test user and returns their ID.
    /// </summary>
    public static async Task<string> RegisterTestUserAsync(
        HttpClient client,
        string? email = null,
        string role = "User")
    {
        email ??= $"test-{Guid.NewGuid():N}@test.local";
        var displayName = $"Test User {email[..8]}";

        var res = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = "TestPass123!",
            displayName,
            role
        });

        res.EnsureSuccessStatusCode();
        var user = await res.Content.ReadFromJsonAsync<UserDto>(JsonOpts);
        return user!.Id;
    }

    /// <summary>
    /// Registers a user or, if the email already exists (409), logs in to recover
    /// the existing user ID. Used for persistent test infrastructure accounts.
    /// </summary>
    public static async Task<string> RegisterOrLoginAsync(
        HttpClient client,
        string email,
        string password = "TestPass123!",
        string role = "User")
    {
        var displayName = $"Test User {email[..8]}";
        var registerRes = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password,
            displayName,
            role
        });

        if (registerRes.IsSuccessStatusCode)
        {
            var registered = await registerRes.Content.ReadFromJsonAsync<UserDto>(JsonOpts);
            return registered!.Id;
        }

        if ((int)registerRes.StatusCode == 409)
        {
            // User already exists — log in to get their ID
            var loginRes = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
            loginRes.EnsureSuccessStatusCode();
            var loggedIn = await loginRes.Content.ReadFromJsonAsync<UserDto>(JsonOpts);
            return loggedIn!.Id;
        }

        registerRes.EnsureSuccessStatusCode(); // throw for any other failure
        throw new InvalidOperationException("Unreachable");
    }

    /// <summary>
    /// Creates an HttpClient with the X-User-Id header pre-set for the given user.
    /// </summary>
    public static HttpClient CreateAuthenticatedClient(AspireFixture fixture, string userId)
        => fixture.CreateAuthenticatedClient(userId);

    private record UserDto(string Id, string Email, string DisplayName, string? AvatarUrl, string[] Roles, DateTime CreatedAt);
}
