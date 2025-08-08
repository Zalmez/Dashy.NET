# Version Management

## Overview

The application uses an intelligent versioning system that:

1. **Automatically displays version information** for each service in the settings panel
2. **Detects changes since the last release** to deploy only affected services
3. **Provides independent versioning** for each service component
4. **Reduces deployment overhead** by skipping unchanged services

## How it Works

### Multi-Service Version Information

Each service (WebFrontend, ApiService, MigrationService) now has independent version tracking:

- **ServiceVersionInfo**: Contains version, build date, commit hash, and pre-release status for each service
- **ApplicationVersionInfo**: Aggregates version information from all services
- **Version Info Tab**: New settings tab showing detailed version information for all services

### Intelligent Deployment

The new GitHub Action workflow (`IntelligentDeployment.yml`) analyzes changes since the last release:

1. **Change Detection**: Compares files changed since the previous Git tag
2. **Service Mapping**: Maps changed files to affected services
3. **Conditional Deployment**: Only builds and deploys services with changes
4. **Independent Versioning**: Each service gets its own version based on changes

### Directory.Build.props

The `Directory.Build.props` file configures MSBuild to:

- Use Git tag information when available (via `GITHUB_REF_NAME` environment variable)
- Fall back to `0.1.0-dev` for local development
- Generate proper assembly version attributes with commit information

### Enhanced Version Service

The `IVersionService` now provides:

- `GetServiceVersionInfo()` - Returns version info for the current service
- `GetApplicationVersionInfoAsync()` - Returns version info for all services
- Cross-service version retrieval via HTTP endpoints

## User Interface

### Version Info Tab

A new tab in Settings → "Version Info" displays:

- **Overall Application Version**: The highest version among all services
- **Service Grid**: Individual cards for each service showing:
  - Service name with appropriate icon
  - Current version with release/development status
  - Full version string with build metadata
  - Git commit hash (when available)
  - Build timestamp
  - Development vs Release status indicator

### Visual Indicators

- **Green badges**: Release versions
- **Yellow badges**: Development/pre-release versions
- **Service icons**: Globe (WebFrontend), Server (ApiService), Database (MigrationService)
- **Status indicators**: Check mark (Release), Flask (Development)

## Release Process

### Intelligent Release (Recommended)

1. **Create a Git tag** (e.g., `v1.0.0`)
2. **GitHub Actions automatically**:
   - Analyzes changes since the previous tag
   - Identifies which services need updating
   - Builds and deploys only changed services
   - Generates a deployment summary

### Manual Release

Use the workflow dispatch option to:
- Force deployment of all services (`force_all: true`)
- Deploy with a custom tag name
- Override the automatic change detection

### Service-Specific Changes

The system maps file changes to services:

- **ApiService**: Changes in `Dashy.Net.ApiService/`, `Dashy.net.Shared/`
- **WebFrontend**: Changes in `Dashy.Net.Web/`, `Dashy.net.Shared/`
- **MigrationService**: Changes in `Dashy.Net.MigrationService/`, `Dashy.net.Shared/`
- **All Services**: Changes in `Directory.Build.props`, workflow files

## Benefits

### Reduced Overhead

- **Faster deployments**: Only changed services are rebuilt
- **Smaller releases**: Users only download updates for changed services
- **Resource efficiency**: Less CI/CD compute time and storage

### Better Tracking

- **Clear service versions**: Each service has independent version history
- **Change visibility**: Easy to see what changed in each release
- **Deployment transparency**: Summary shows exactly what was deployed

### Development Experience

- **Version visibility**: Developers and users can see exact service versions
- **Debug information**: Commit hashes and build dates aid troubleshooting
- **Status clarity**: Clear indicators for development vs production builds

## Configuration Files

### GitHub Actions

- `IntelligentDeployment.yml` - Main intelligent deployment workflow
- `DockerImages.yml` - Legacy deployment workflow (still functional)

### Application Files

- `Dashy.Net.Web/Services/VersionService.cs` - Enhanced version service
- `Dashy.net.Shared/Models/ServiceVersionInfo.cs` - Version data models  
- `Dashy.Net.ApiService/Controllers/VersionController.cs` - API version endpoint
- `Dashy.Net.Web/Components/Shared/SettingsModal.razor` - Version info UI

### Version Format

- Release tags should follow semantic versioning: `v1.0.0`, `v1.2.3`, etc.
- The `v` prefix is automatically removed for display
- Pre-release versions can include suffixes: `v1.0.0-beta1`, `v1.0.0-rc1`
- Development versions display as `0.1.0-dev` with pre-release indicators

## Local Development

During local development:

- Version displays as `0.1.0-dev`
- Pre-release disclaimer is shown
- No manual version updates needed

## GitHub Actions Integration

The `DockerImages.yml` workflow:

- Passes version information as build arguments to Docker
- Sets `GITHUB_REF_NAME` and `GITHUB_REF_TYPE` environment variables
- Ensures consistent versioning across all services

## Manual Override

If needed, you can manually set the version by:

1. Setting environment variables:
   ```
   GITHUB_REF_NAME=1.2.3
   GITHUB_REF_TYPE=tag
   ```
2. Or modifying the `Version` property in `Directory.Build.props`

## Files Modified

- `Directory.Build.props` - Version configuration
- `Dashy.Net.Web/Services/VersionService.cs` - Version service implementation
- `Dashy.Net.Web/Program.cs` - Service registration
- `Dashy.Net.Web/Components/Layout/MainLayout.razor` - Frontend display
- `.github/workflows/DockerImages.yml` - CI/CD integration
- All `Dockerfile`s - Build argument support
