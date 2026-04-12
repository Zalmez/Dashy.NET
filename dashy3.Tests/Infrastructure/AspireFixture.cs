using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace dashy3.Tests.Infrastructure;

/// <summary>
/// Shared fixture that starts the Aspire AppHost ONCE for the entire test run.
/// All test classes should use [Collection("Aspire")] — NOT IClassFixture&lt;AspireFixture&gt;.
/// </summary>
public class AspireFixture : IAsyncLifetime
{
    private DistributedApplication? _app;
    private string? _cleanupAdminId;
    private HttpClient? _cleanupAdminClient;
    private string? _previousTestingFlag;

    // CI cold starts (container pulls/builds, health checks) can easily exceed 2 minutes.
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(8);

    public DistributedApplication App => _app ?? throw new InvalidOperationException("Fixture not initialized.");

    public async Task InitializeAsync()
    {
        using var cts = new CancellationTokenSource(DefaultTimeout);
        var ct = cts.Token;

        _previousTestingFlag = Environment.GetEnvironmentVariable("DASHY3_TESTING");
        Environment.SetEnvironmentVariable("DASHY3_TESTING", "true");

        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.dashy3_AppHost>(ct);

        appHost.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Warning);
        });

        // Do NOT add StandardResilienceHandler — it causes slow retries in tests
        // Fast failure is preferable for integration tests

        _app = await appHost.BuildAsync(ct);
        await _app.StartAsync(ct);

        // Wait for the API service to be healthy before any tests run
        await _app.ResourceNotifications
            .WaitForResourceHealthyAsync("apiservice", ct);

        // Wait for webfrontend too (used by WebSmokeTests)
        await _app.ResourceNotifications
            .WaitForResourceHealthyAsync("webfrontend", ct);

        // Create a dedicated admin used only for test cleanup.
        // If the user already exists from a prior run, log in to recover the ID.
        var anon = CreateApiClient();
        _cleanupAdminId = await TestAuthHelper.RegisterOrLoginAsync(
            anon, email: "cleanup-admin@test.local", password: "TestPass123!", role: "Admin");
        _cleanupAdminClient = CreateAuthenticatedClient(_cleanupAdminId);
    }

    public async Task DisposeAsync()
    {
        _cleanupAdminClient?.Dispose();
        if (_app is not null)
            await _app.DisposeAsync();

        Environment.SetEnvironmentVariable("DASHY3_TESTING", _previousTestingFlag);
    }

    /// <summary>Creates an HttpClient targeting the apiservice resource.</summary>
    public HttpClient CreateApiClient() => App.CreateHttpClient("apiservice");

    /// <summary>Creates an HttpClient targeting the webfrontend resource.</summary>
    public HttpClient CreateWebClient() => App.CreateHttpClient("webfrontend");

    /// <summary>Creates an authenticated HttpClient with X-User-Id set.</summary>
    public HttpClient CreateAuthenticatedClient(string userId)
    {
        var client = CreateApiClient();
        client.DefaultRequestHeaders.TryAddWithoutValidation("X-User-Id", userId);
        return client;
    }

    /// <summary>
    /// Deletes each user's owned dashboards then deletes the users themselves.
    /// Safe to call with IDs that have already been deleted (404s are ignored).
    /// </summary>
    public async Task DeleteUsersAsync(IEnumerable<string> userIds)
    {
        if (_cleanupAdminClient is null) return;

        foreach (var userId in userIds)
        {
            if (userId == _cleanupAdminId) continue; // never delete our own cleanup admin

            // Delete all dashboards owned by this user first (no FK cascade from Identity users)
            try
            {
                using var userClient = CreateAuthenticatedClient(userId);
                var dashboards = await userClient.GetFromJsonAsync<List<DashboardSummary>>("/api/dashboards");
                if (dashboards is not null)
                    foreach (var d in dashboards.Where(d => d.OwnerId == userId))
                        await userClient.DeleteAsync($"/api/dashboards/{d.Id}");
            }
            catch { /* best effort — dashboards may already be gone */ }

            // Delete the Identity user
            try { await _cleanupAdminClient.DeleteAsync($"/api/admin/users/{userId}"); }
            catch { /* best effort */ }
        }
    }

    private record DashboardSummary(string Id, string Name, string? OwnerId);
}
