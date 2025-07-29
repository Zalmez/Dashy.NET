using Aspire.Hosting.Testing;
using System.Net;
using Xunit;

namespace Dashy.Net.Tests.Integration;

public class DashboardIntegrationTests
{
    [Fact]
    public async Task ApiService_HealthCheck_ReturnsHealthy()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Dashy_Net_AppHost>();
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        var httpClient = app.CreateHttpClient("apiservice");
        var response = await httpClient.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ApiService_DashboardConfig_ReturnsData()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Dashy_Net_AppHost>();
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        var httpClient = app.CreateHttpClient("apiservice");
        var response = await httpClient.GetAsync("/api/dashboard/config");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(content);
    }

    [Fact]
    public async Task ApiService_WeatherEndpoint_ReturnsData()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Dashy_Net_AppHost>();
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        var httpClient = app.CreateHttpClient("apiservice");
        var response = await httpClient.GetAsync("/api/weather?latitude=52.52&longitude=13.41&units=celsius");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(content);
    }

    [Fact]
    public async Task ApiService_DashboardList_ReturnsData()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Dashy_Net_AppHost>();
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        var httpClient = app.CreateHttpClient("apiservice");
        var response = await httpClient.GetAsync("/api/dashboard/list");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(content);
    }

    [Fact]
    public async Task ApiService_SeedAndCheckConfig_ReturnsData()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Dashy_Net_AppHost>();
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        var httpClient = app.CreateHttpClient("apiservice");

        // First seed the database
        var seedResponse = await httpClient.PostAsync("/api/dashboard/seed", null);
        Assert.True(seedResponse.IsSuccessStatusCode);

        // Then check that config returns data
        var configResponse = await httpClient.GetAsync("/api/dashboard/config");
        Assert.Equal(HttpStatusCode.OK, configResponse.StatusCode);
        var content = await configResponse.Content.ReadAsStringAsync();
        Assert.NotEmpty(content);
    }

    [Fact]
    public async Task WebApp_HealthCheck_ReturnsHealthy()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Dashy_Net_AppHost>();
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        var httpClient = app.CreateHttpClient("webfrontend");
        var response = await httpClient.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
