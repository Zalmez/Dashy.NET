namespace Dashy.Net.Shared.Models;

/// <summary>
/// Represents version information for a specific service.
/// </summary>
public class ServiceVersionInfo
{
    /// <summary>
    /// The name of the service (e.g., "ApiService", "WebFrontend", "MigrationService").
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// The current version of the service.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// The full version string including build metadata.
    /// </summary>
    public string FullVersion { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is a development/pre-release version.
    /// </summary>
    public bool IsPreRelease { get; set; }

    /// <summary>
    /// The Git commit hash this version was built from.
    /// </summary>
    public string? CommitHash { get; set; }

    /// <summary>
    /// The build timestamp.
    /// </summary>
    public DateTime? BuildDate { get; set; }
}

/// <summary>
/// Represents version information for all services in the application.
/// </summary>
public class ApplicationVersionInfo
{
    /// <summary>
    /// Version information for each service.
    /// </summary>
    public List<ServiceVersionInfo> Services { get; set; } = new();

    /// <summary>
    /// The overall application version (typically matches the latest service version).
    /// </summary>
    public string ApplicationVersion { get; set; } = string.Empty;

    /// <summary>
    /// Whether any service is running a pre-release version.
    /// </summary>
    public bool HasPreReleaseServices => Services.Any(s => s.IsPreRelease);

    /// <summary>
    /// Gets version info for a specific service.
    /// </summary>
    /// <param name="serviceName">The name of the service to get version info for.</param>
    /// <returns>The service version info, or null if not found.</returns>
    public ServiceVersionInfo? GetServiceVersion(string serviceName)
    {
        return Services.FirstOrDefault(s => s.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase));
    }
}
