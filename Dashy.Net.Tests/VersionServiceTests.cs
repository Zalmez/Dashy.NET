using Dashy.Net.Web.Services;

namespace Dashy.Net.Tests;

public class VersionServiceTests
{
    [Fact]
    public void VersionService_ShouldNotReturnUnknownVersion()
    {
        // Arrange
        var versionService = new VersionService();

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
        // Arrange
        var versionService = new VersionService();

        // Act & Assert - Should not throw exceptions
        var isPreRelease = versionService.IsPreRelease();
        
        // Basic type check - should return a boolean
        Assert.IsType<bool>(isPreRelease);
    }
}
