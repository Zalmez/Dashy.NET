using Dashy.Net.Web.Services;
using Xunit;

namespace Dashy.Net.Tests.Unit.Services;

public class VersionServiceTests
{
    [Fact]
    public void GetVersion_ReturnsValidVersion()
    {
        var service = new VersionService();

        var version = service.GetVersion();

        Assert.NotNull(version);
        Assert.NotEmpty(version);
        Assert.NotEqual("Unknown", version);
    }

    [Fact]
    public void GetFullVersion_ReturnsValidVersion()
    {
        var service = new VersionService();

        var fullVersion = service.GetFullVersion();

        Assert.NotNull(fullVersion);
        Assert.NotEmpty(fullVersion);
        Assert.NotEqual("Unknown", fullVersion);
    }

    [Fact]
    public void IsPreRelease_ReturnsBooleanValue()
    {
        var service = new VersionService();

        var isPreRelease = service.IsPreRelease();

        Assert.IsType<bool>(isPreRelease);
    }

    [Fact]
    public void GetVersion_ConsistentResults()
    {
        var service = new VersionService();

        var version1 = service.GetVersion();
        var version2 = service.GetVersion();

        Assert.Equal(version1, version2);
    }

    [Fact]
    public void GetFullVersion_ConsistentResults()
    {
        var service = new VersionService();

        var fullVersion1 = service.GetFullVersion();
        var fullVersion2 = service.GetFullVersion();

        Assert.Equal(fullVersion1, fullVersion2);
    }

    [Fact]
    public void IsPreRelease_ConsistentResults()
    {
        var service = new VersionService();

        var isPreRelease1 = service.IsPreRelease();
        var isPreRelease2 = service.IsPreRelease();

        Assert.Equal(isPreRelease1, isPreRelease2);
    }
}