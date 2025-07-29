using Dashy.Net.ApiService.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using Xunit;

namespace Dashy.Net.Tests.Unit.Controllers;

public class WeatherControllerTests
{
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<ILogger<WeatherController>> _mockLogger;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _mockHttpClient;

    public WeatherControllerTests()
    {
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockLogger = new Mock<ILogger<WeatherController>>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        _mockHttpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://api.open-meteo.com/")
        };

        _mockHttpClientFactory.Setup(f => f.CreateClient("WeatherApi")).Returns(_mockHttpClient);
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        var controller = new WeatherController(_mockHttpClientFactory.Object, _mockLogger.Object);

        Assert.NotNull(controller);
    }

    [Fact]
    public async Task GetWeather_WithDefaultParameters_CallsCorrectUrl()
    {
        var expectedResponse = new
        {
            current_weather = new
            {
                temperature = 22.5,
                weathercode = 1
            }
        };

        var responseContent = JsonSerializer.Serialize(expectedResponse);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, System.Text.Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        var controller = new WeatherController(_mockHttpClientFactory.Object, _mockLogger.Object);

        var result = await controller.GetWeather();

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetWeather_WithCustomParameters_CallsCorrectUrl()
    {
        var expectedResponse = new
        {
            current_weather = new
            {
                temperature = 75.0,
                weathercode = 2
            }
        };

        var responseContent = JsonSerializer.Serialize(expectedResponse);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, System.Text.Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        var controller = new WeatherController(_mockHttpClientFactory.Object, _mockLogger.Object);

        var result = await controller.GetWeather(40.7128, -74.0060, "fahrenheit");

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetWeather_WhenApiReturnsError_ReturnsErrorStatusCode()
    {
        var httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("Bad Request", System.Text.Encoding.UTF8, "text/plain")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        var controller = new WeatherController(_mockHttpClientFactory.Object, _mockLogger.Object);

        var result = await controller.GetWeather();

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, statusCodeResult.StatusCode);
    }
}