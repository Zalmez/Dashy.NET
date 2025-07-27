# Version Management

## Overview

The application automatically displays the current version in the footer. The version is determined from:

1. **Git tags** during release builds (via GitHub Actions)
2. **Development fallback** during local development

## How it Works

### Directory.Build.props

The `Directory.Build.props` file at the solution root configures MSBuild to:

- Use Git tag information when available (via `GITHUB_REF_NAME` environment variable)
- Fall back to `0.1.0-dev` for local development
- Generate proper assembly version attributes

### Version Service

The `IVersionService` in the Web project reads version information from assembly attributes:

- `GetVersion()` - Returns the simple version (e.g., "1.2.3")
- `GetFullVersion()` - Returns full version with build metadata
- `IsPreRelease()` - Determines if this is a pre-release version

### Frontend Display

The `MainLayout.razor` uses the `IVersionService` to:

- Display the current version in the footer
- Show disclaimer text only for pre-release versions
- Update automatically without manual intervention

## Release Process

### Creating a Release

1. Create and push a Git tag (e.g., `v1.0.0`)
2. GitHub Actions will automatically:
   - Build the application with the correct version
   - Create Docker images tagged with the version
   - The frontend will display the release version

### Version Format

- Release tags should follow semantic versioning: `v1.0.0`, `v1.2.3`, etc.
- The `v` prefix is automatically removed for display
- Pre-release versions can include suffixes: `v1.0.0-beta1`, `v1.0.0-rc1`

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
