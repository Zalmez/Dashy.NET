using Dashy.Net.Web.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Http;

namespace Dashy.Net.Tests;

public class VersionServiceTests
{
    private static IHttpClientFactory CreateMockHttpClientFactory()
    {
        return new TestHttpClientFactory();
    }

    private static ILogger<VersionService> CreateMockLogger()
    {
        return new TestLogger<VersionService>();
    }

    [Fact]
    public void VersionService_ShouldNotReturnUnknownVersion()
    {
        // Arrange
        var httpClientFactory = CreateMockHttpClientFactory();
        var logger = CreateMockLogger();
        var versionService = new VersionService(httpClientFactory, logger);

        // Act
        var version = versionService.GetVersion();
        var fullVersion = versionService.GetFullVersion();

        // Assert - Basic sanity checks
        Assert.NotNull(version);
        Assert.NotEmpty(version);
        Assert.NotEqual("Unknown", version);
        
        Assert.NotNull(fullVersion);
        Assert.NotEmpty(fullVersion);
        Assert.NotEqual("Unknown", fullVersion);
    }

    [Fact]
    public void VersionService_PreReleaseDetection_ShouldNotThrow()
    {
        var httpClientFactory = CreateMockHttpClientFactory();
        var logger = CreateMockLogger();
        var versionService = new VersionService(httpClientFactory, logger);

        var isPreRelease = versionService.IsPreRelease();
        
        Assert.IsType<bool>(isPreRelease);
    }
}
public class TestHttpClientFactory : IHttpClientFactory
{
    public HttpClient CreateClient(string name)
    {
        return new HttpClient();
    }
}

public class TestLogger<T> : ILogger<T>
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => new TestScope();
    public bool IsEnabled(LogLevel logLevel) => false;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
}

public class TestScope : IDisposable
{
    public void Dispose() { }
}
