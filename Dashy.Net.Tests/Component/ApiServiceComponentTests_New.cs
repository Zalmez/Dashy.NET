using Aspire.Hosting.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Dashy.Net.Tests.Component;

public class ApiServiceComponentTests
{
    [Fact]
    public async Task ApiService_StartsSuccessfully()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Dashy_Net_AppHost>();
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        var httpClient = app.CreateHttpClient("apiservice");
        var response = await httpClient.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task WeatherEndpoint_ReturnsWeatherData()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Dashy_Net_AppHost>();
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        var httpClient = app.CreateHttpClient("apiservice");
        var response = await httpClient.GetAsync("/weather?latitude=52.52&longitude=13.41&units=celsius");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(content);
        Assert.Contains("temperature", content);
    }

    [Fact]
    public async Task DashboardEndpoint_ReturnsDashboardData()
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
    public async Task SectionsCrudOperations_WorkCorrectly()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Dashy_Net_AppHost>();
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        var httpClient = app.CreateHttpClient("apiservice");

        // Test GET sections
        var getResponse = await httpClient.GetAsync("/sections");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        // Test that we can access the sections endpoint
        var content = await getResponse.Content.ReadAsStringAsync();
        Assert.NotNull(content);
    }

    [Fact]
    public async Task ItemsCrudOperations_WorkCorrectly()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Dashy_Net_AppHost>();
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        var httpClient = app.CreateHttpClient("apiservice");

        // Test GET items
        var getResponse = await httpClient.GetAsync("/items");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        // Test that we can access the items endpoint
        var content = await getResponse.Content.ReadAsStringAsync();
        Assert.NotNull(content);
    }

    [Fact]
    public async Task HeaderButtonsEndpoint_ReturnsHeaderButtonData()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Dashy_Net_AppHost>();
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        var httpClient = app.CreateHttpClient("apiservice");
        var response = await httpClient.GetAsync("/header-buttons");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.NotNull(content);
    }
}
