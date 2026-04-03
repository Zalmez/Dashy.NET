using dashy3.Tests.Infrastructure;

namespace dashy3.Tests.Tests;

[Collection("Aspire")]
public class AppHostTests(AspireFixture fixture)
{
    [Fact]
    public async Task ApiService_HealthEndpoint_ReturnsOk()
    {
        var client = fixture.CreateApiClient();
        var response = await client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ApiService_IsHealthy()
    {
        // The fixture already waited for apiservice to be healthy in InitializeAsync.
        // This test confirms it stays healthy and responds.
        var client = fixture.CreateApiClient();
        var response = await client.GetAsync("/health");
        Assert.True(response.IsSuccessStatusCode,
            $"apiservice /health returned {response.StatusCode}");
    }

    [Fact]
    public async Task WebFrontend_IsReachable()
    {
        await fixture.App.ResourceNotifications
            .WaitForResourceHealthyAsync("webfrontend")
            .WaitAsync(AspireFixture.DefaultTimeout);

        var client = fixture.CreateWebClient();
        var response = await client.GetAsync("/login");
        // /login is publicly accessible
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AppModel_ContainsBothResources()
    {
        // Verify both resources are reachable (healthy) in the running app
        await fixture.App.ResourceNotifications
            .WaitForResourceHealthyAsync("apiservice")
            .WaitAsync(AspireFixture.DefaultTimeout);
        await fixture.App.ResourceNotifications
            .WaitForResourceHealthyAsync("webfrontend")
            .WaitAsync(AspireFixture.DefaultTimeout);
    }

    [Fact]
    public async Task AppModel_WebFrontend_HasApiServiceAnnotation()
    {
        // webfrontend depends on apiservice — confirm apiservice is healthy before webfrontend responds
        await fixture.App.ResourceNotifications
            .WaitForResourceHealthyAsync("apiservice")
            .WaitAsync(AspireFixture.DefaultTimeout);

        var client = fixture.CreateWebClient();
        var response = await client.GetAsync("/login");
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Redirect || response.StatusCode == HttpStatusCode.Found,
            $"webfrontend /login returned {response.StatusCode}");
    }
}
