namespace dashy3.Tests.Infrastructure;

/// <summary>
/// Per-test cleanup scope. Tracks user IDs created during a test and deletes
/// them (plus their owned dashboards) when the test finishes.
///
/// Usage:
///   await using var scope = new TestScope(fixture);
///   var (userId, client) = await scope.CreateUserAsync();
/// </summary>
public sealed class TestScope(AspireFixture fixture) : IAsyncDisposable
{
    private readonly List<string> _userIds = [];

    /// <summary>
    /// Registers a unique test user, tracks it for cleanup, and returns an
    /// authenticated HttpClient for that user.
    /// </summary>
    public async Task<(string userId, HttpClient client)> CreateUserAsync(
        string? email = null,
        string role = "User")
    {
        var anon = fixture.CreateApiClient();
        var userId = await TestAuthHelper.RegisterTestUserAsync(anon, email, role);
        _userIds.Add(userId);
        return (userId, fixture.CreateAuthenticatedClient(userId));
    }

    /// <summary>
    /// Tracks an externally-created user ID for cleanup (e.g. when the test
    /// registers a user via raw HTTP and captures the ID from the response).
    /// </summary>
    public void Track(string userId) => _userIds.Add(userId);

    public ValueTask DisposeAsync() => new(fixture.DeleteUsersAsync(_userIds));
}
