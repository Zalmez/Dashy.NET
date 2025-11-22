using System.Reflection;
using Dashy.Net.Shared.Models;

namespace Dashy.Net.Web.Services;

/// <summary>
/// Service for accessing application version information.
/// </summary>
public interface IVersionService
{
    /// <summary>
    /// Gets the application version from the assembly.
    /// </summary>
    string GetVersion();

    /// <summary>
    /// Gets the full version information including build metadata.
    /// </summary>
    string GetFullVersion();

    /// <summary>
    /// Indicates whether this is a development/pre-release version.
    /// </summary>
    bool IsPreRelease();

    /// <summary>
    /// Gets version information for this service (WebFrontend).
    /// </summary>
    ServiceVersionInfo GetServiceVersionInfo();

    /// <summary>
    /// Gets version information for all services in the application.
    /// </summary>
    Task<ApplicationVersionInfo> GetApplicationVersionInfoAsync();
}

/// <summary>
/// Implementation of IVersionService that reads version from assembly attributes.
/// </summary>
public class VersionService : IVersionService
{
    private readonly string _version;
    private readonly string _fullVersion;
    private readonly bool _isPreRelease;
    private readonly HttpClient _httpClient;
    private readonly ILogger<VersionService> _logger;

    public VersionService(IHttpClientFactory httpClientFactory, ILogger<VersionService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("ApiService");
        _logger = logger;
        var assembly = Assembly.GetExecutingAssembly();

        // Get the informational version (includes build metadata)
        var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        _fullVersion = informationalVersion ?? "Unknown";

        // Try to extract a clean version from informational version first
        if (!string.IsNullOrEmpty(informationalVersion))
        {
            // Remove build metadata (everything after '+')
            var versionPart = informationalVersion.Split('+')[0];
            _version = versionPart;
        }
        else
        {
            // Fallback to file version or assembly version
            var fileVersion = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
            var assemblyVersion = assembly.GetName().Version;

            if (!string.IsNullOrEmpty(fileVersion))
            {
                _version = fileVersion;
            }
            else if (assemblyVersion != null)
            {
                _version = $"{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}";
            }
            else
            {
                _version = "Unknown";
            }
        }

        // Determine if this is a pre-release version
        _isPreRelease = _version.Contains("-") ||
                       _version.Contains("dev") ||
                       _fullVersion.Contains("-") ||
                       _fullVersion.Contains("dev") ||
                       _version == "0.1.0-dev" ||
                       _version.StartsWith("0.1.0");
    }

    public string GetVersion()
    {
        return _version;
    }

    public string GetFullVersion()
    {
        return _fullVersion;
    }

    public bool IsPreRelease()
    {
        return _isPreRelease;
    }

    public ServiceVersionInfo GetServiceVersionInfo()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var commitHash = GetCommitHashFromAssembly(assembly);
        var buildDate = GetBuildDateFromAssembly(assembly);

        return new ServiceVersionInfo
        {
            ServiceName = "WebFrontend",
            Version = _version,
            FullVersion = _fullVersion,
            IsPreRelease = _isPreRelease,
            CommitHash = commitHash,
            BuildDate = buildDate
        };
    }

    public async Task<ApplicationVersionInfo> GetApplicationVersionInfoAsync()
    {
        var services = new List<ServiceVersionInfo>();

        // Add the current service (WebFrontend)
        services.Add(GetServiceVersionInfo());

        try
        {
            // Try to get version info from API service
            var apiVersionInfo = await GetApiServiceVersionAsync();
            if (apiVersionInfo != null)
            {
                services.Add(apiVersionInfo);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve version information from some services");
        }

        // Determine the overall application version (use the highest version among services)
        var applicationVersion = services
            .Select(s => s.Version)
            .Where(v => !string.IsNullOrEmpty(v) && v != "Unknown")
            .OrderByDescending(v => v)
            .FirstOrDefault() ?? _version;

        return new ApplicationVersionInfo
        {
            Services = services,
            ApplicationVersion = applicationVersion
        };
    }

    private async Task<ServiceVersionInfo?> GetApiServiceVersionAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/version");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                // Parse the response and create ServiceVersionInfo
                // For now, return a basic version info
                return new ServiceVersionInfo
                {
                    ServiceName = "ApiService",
                    Version = _version, // Will be updated when API endpoint is implemented
                    FullVersion = _fullVersion,
                    IsPreRelease = _isPreRelease
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Could not retrieve version from API service");
        }
        return null;
    }

    private string? GetCommitHashFromAssembly(Assembly assembly)
    {
        try
        {
            // Try to extract commit hash from informational version
            var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            if (!string.IsNullOrEmpty(informationalVersion) && informationalVersion.Contains('+'))
            {
                var parts = informationalVersion.Split('+');
                if (parts.Length > 1)
                {
                    return parts[1].Split('.')[0]; // Get the first part after '+' which should be commit hash
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Could not extract commit hash from assembly");
        }
        return null;
    }

    private DateTime? GetBuildDateFromAssembly(Assembly assembly)
    {
        try
        {
            // For now, return the assembly creation time as build date
            var location = assembly.Location;
            if (!string.IsNullOrEmpty(location) && File.Exists(location))
            {
                return File.GetCreationTimeUtc(location);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Could not determine build date from assembly");
        }
        return null;
    }
}
