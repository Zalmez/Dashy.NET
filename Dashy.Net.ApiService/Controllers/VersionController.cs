using Dashy.Net.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace Dashy.Net.ApiService.Controllers;

/// <summary>
/// Controller for version information endpoints.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class VersionController : ControllerBase
{
    private readonly ILogger<VersionController> _logger;

    public VersionController(ILogger<VersionController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Gets the version information for the API service.
    /// </summary>
    /// <returns>The service version information.</returns>
    [HttpGet]
    public ActionResult<ServiceVersionInfo> GetVersion()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = GetVersionFromAssembly(assembly);
            var fullVersion = GetFullVersionFromAssembly(assembly);
            var isPreRelease = IsPreReleaseVersion(version, fullVersion);
            var commitHash = GetCommitHashFromAssembly(assembly);
            var buildDate = GetBuildDateFromAssembly(assembly);

            var versionInfo = new ServiceVersionInfo
            {
                ServiceName = "ApiService",
                Version = version,
                FullVersion = fullVersion,
                IsPreRelease = isPreRelease,
                CommitHash = commitHash,
                BuildDate = buildDate
            };

            return Ok(versionInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve version information");
            return StatusCode(500, "Failed to retrieve version information");
        }
    }

    private static string GetVersionFromAssembly(Assembly assembly)
    {
        // Get the informational version (includes build metadata)
        var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        if (!string.IsNullOrEmpty(informationalVersion))
        {
            // Remove build metadata (everything after '+')
            var versionPart = informationalVersion.Split('+')[0];
            return versionPart;
        }

        // Fallback to file version or assembly version
        var fileVersion = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
        var assemblyVersion = assembly.GetName().Version;

        if (!string.IsNullOrEmpty(fileVersion))
        {
            return fileVersion;
        }

        if (assemblyVersion != null)
        {
            return $"{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}";
        }

        return "Unknown";
    }

    private static string GetFullVersionFromAssembly(Assembly assembly)
    {
        var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        return informationalVersion ?? "Unknown";
    }

    private static bool IsPreReleaseVersion(string version, string fullVersion)
    {
        return version.Contains("-") ||
               version.Contains("dev") ||
               fullVersion.Contains("-") ||
               fullVersion.Contains("dev") ||
               version == "0.1.0-dev" ||
               version.StartsWith("0.1.0");
    }

    private string? GetCommitHashFromAssembly(Assembly assembly)
    {
        try
        {
            var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            if (!string.IsNullOrEmpty(informationalVersion) && informationalVersion.Contains('+'))
            {
                var parts = informationalVersion.Split('+');
                if (parts.Length > 1)
                {
                    return parts[1].Split('.')[0];
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
            var location = assembly.Location;
            if (!string.IsNullOrEmpty(location) && System.IO.File.Exists(location))
            {
                return System.IO.File.GetCreationTimeUtc(location);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Could not determine build date from assembly");
        }
        return null;
    }
}
