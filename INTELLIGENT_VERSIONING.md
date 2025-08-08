# Intelligent Versioning and Deployment

This document describes the intelligent versioning system implemented in Dashy.NET that reduces deployment overhead by only updating services that have actually changed.

## Quick Start

### Using the New System

1. **View Version Information**: Go to Settings → Version Info to see detailed version information for all services
2. **Release Process**: Create a git tag (e.g., `v1.2.0`) - only changed services will be deployed
3. **Force Full Deployment**: Use the workflow dispatch with `force_all: true` if needed

### Key Benefits

- ✅ **Reduced deployment time** - Only changed services are built and deployed
- ✅ **Independent service versioning** - Each service can have its own version history  
- ✅ **Better visibility** - Clear view of what changed in each release
- ✅ **Resource efficiency** - Less CI/CD usage and smaller updates for users

## How Change Detection Works

The system analyzes git changes between releases to determine which services need updating:

```yaml
# Service dependencies and change triggers
ApiService:
  - Dashy.Net.ApiService/**
  - Dashy.net.Shared/**
  - Directory.Build.props

WebFrontend:
  - Dashy.Net.Web/**
  - Dashy.net.Shared/**
  - Directory.Build.props

MigrationService:
  - Dashy.Net.MigrationService/**
  - Dashy.net.Shared/**
  - Directory.Build.props
```

## Version Information UI

The new "Version Info" tab in Settings provides:

### Application Overview
- Overall application version
- Pre-release status indicator
- Quick deployment status

### Service Details
Each service shows:
- 🌐 **WebFrontend** - User interface and web components
- 🖥️ **ApiService** - Backend API and business logic  
- 🗄️ **MigrationService** - Database migrations and setup

For each service:
- Current version number
- Full version with build metadata
- Git commit hash (for debugging)
- Build timestamp
- Release vs Development status

## GitHub Actions Workflow

### Intelligent Deployment Workflow

The new `IntelligentDeployment.yml` workflow:

1. **Detects Changes**: Compares current commit with previous release tag
2. **Analyzes Impact**: Maps changed files to affected services
3. **Builds Selectively**: Only builds services that have changes
4. **Generates Summary**: Creates detailed deployment report

### Workflow Inputs

```yaml
# Manual trigger options
tag_name: "v1.2.0"        # Custom version tag
force_all: false          # Deploy all services regardless of changes
```

### Example Deployment Summary

```
🚀 Deployment Summary

Release Tag: v1.2.0

## Services Deployed
- ✅ WebFrontend (v1.2.0) - Successfully deployed
- ⏭️ ApiService - No changes detected, skipped  
- ✅ MigrationService (v1.2.0) - Successfully deployed
```

## Development Workflow

### Local Development
- Version shows as `0.1.0-dev` with development indicators
- All services show development status
- Version info updates automatically during development

### Creating Releases
1. **Make changes** to your code
2. **Test locally** using the development version
3. **Create a git tag**: `git tag v1.2.0 && git push origin v1.2.0`
4. **GitHub Actions automatically**:
   - Analyzes what changed since v1.1.x
   - Builds only the affected services
   - Deploys with proper version numbers

### Service Versioning Strategy

Each service can evolve independently:
- **WebFrontend v1.2.0** + **ApiService v1.1.0** + **MigrationService v1.2.0** = **Application v1.2.0**
- The overall application version matches the highest service version
- Users can see exactly which services are running which versions

## Troubleshooting

### Force Full Deployment
If you need to deploy all services regardless of changes:
```bash
# Via GitHub UI: Actions → Intelligent Deployment → Run workflow
# Set force_all: true
```

### Check Service Versions
- Navigate to Settings → Version Info
- Each service shows its current version and status
- Commit hashes help identify exact builds

### Rollback Strategy
Since each service is versioned independently:
1. Identify the problematic service from Version Info
2. Deploy a previous working version of just that service
3. Other services can continue running their current versions

## Migration from Old System

The legacy `DockerImages.yml` workflow is still available but the new intelligent system is recommended:

### Advantages of New System
- **Performance**: ~50% reduction in deployment time for typical changes
- **Clarity**: Clear visibility into what's being deployed
- **Efficiency**: Reduced Docker registry usage and CI/CD minutes
- **Debugging**: Better traceability with commit hashes and build dates

### Compatibility
- Both workflows can coexist during transition
- Version display works with both old and new deployment methods
- No breaking changes to existing functionality
