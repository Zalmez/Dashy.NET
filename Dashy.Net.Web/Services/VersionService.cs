using System.Reflection;

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
}

/// <summary>
/// Implementation of IVersionService that reads version from assembly attributes.
/// </summary>
public class VersionService : IVersionService
{
    private readonly string _version;
    private readonly string _fullVersion;
    private readonly bool _isPreRelease;

    public VersionService()
    {
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
}
