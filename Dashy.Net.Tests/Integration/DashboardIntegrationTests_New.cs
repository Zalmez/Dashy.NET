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
        var response = await httpClient.GetAsync("/dashboard");

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
        var response = await httpClient.GetAsync("/weather?latitude=52.52&longitude=13.41&units=celsius");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(content);
    }

    [Fact]
    public async Task ApiService_SectionsEndpoint_ReturnsData()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Dashy_Net_AppHost>();
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        var httpClient = app.CreateHttpClient("apiservice");
        var response = await httpClient.GetAsync("/sections");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(content);
    }

    [Fact]
    public async Task ApiService_ItemsEndpoint_ReturnsData()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Dashy_Net_AppHost>();
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        var httpClient = app.CreateHttpClient("apiservice");
        var response = await httpClient.GetAsync("/items");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(content);
    }

    [Fact]
    public async Task WebApp_HealthCheck_ReturnsHealthy()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Dashy_Net_AppHost>();
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        var httpClient = app.CreateHttpClient("web");
        var response = await httpClient.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
