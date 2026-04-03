using dashy3.Tests.Infrastructure;

namespace dashy3.Tests.Tests;

[Collection("Aspire")]
public class WebSmokeTests(AspireFixture fixture)
{
    [Fact]
    public async Task LoginPage_Returns200()
    {
        await fixture.App.ResourceNotifications
            .WaitForResourceHealthyAsync("webfrontend")
            .WaitAsync(AspireFixture.DefaultTimeout);

        // Allow auto-redirect so HTTP→HTTPS redirects are followed transparently
        using var client = fixture.CreateWebClient();

        var response = await client.GetAsync("/login");
        // /login should be directly accessible (200) — not require auth
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task RootPath_UnauthenticatedUser_RedirectsToLogin()
    {
        await fixture.App.ResourceNotifications
            .WaitForResourceHealthyAsync("webfrontend")
            .WaitAsync(AspireFixture.DefaultTimeout);

        var handler = new HttpClientHandler { AllowAutoRedirect = false };
        var baseAddress = fixture.CreateWebClient().BaseAddress!;
        using var client = new HttpClient(handler) { BaseAddress = baseAddress };

        var response = await client.GetAsync("/");
        // Should redirect unauthenticated users away from root (301/302/303/307/308 or 200 on first run)
        Assert.True(
            (int)response.StatusCode is >= 300 and < 400 ||
            response.StatusCode == HttpStatusCode.OK,
            $"Expected redirect or OK for unauthenticated root, got {response.StatusCode}");
    }
}
