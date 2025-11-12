# .NET 10.0 Upgrade Plan

## Execution Steps

Execute steps below sequentially one by one in the order they are listed.

1. Validate that an .NET 10.0 SDK required for this upgrade is installed on the machine and if not, help to get it installed.
2. Ensure that the SDK version specified in global.json files is compatible with the .NET 10.0 upgrade.
3. Upgrade Dashy.Net.AppHost/Dashy.Net.AppHost.csproj
4. Upgrade Dashy.Net.ServiceDefaults/Dashy.Net.ServiceDefaults.csproj
5. Upgrade Dashy.Net.ApiService/Dashy.Net.ApiService.csproj
6. Upgrade Dashy.Net.Web/Dashy.Net.Web.csproj
7. Upgrade Dashy.net.Shared/Dashy.Net.Shared.csproj
8. Upgrade Dashy.Net.MigrationService/Dashy.Net.MigrationService.csproj
9. Upgrade Dashy.Net.Tests/Dashy.Net.Tests.csproj

## Settings

This section contains settings and data used by execution steps.

### Excluded projects

Table below contains projects that do belong to the dependency graph for selected projects and should not be included in the upgrade.

| Project name                                   | Description                 |
|:-----------------------------------------------|:---------------------------:|

### Aggregate NuGet packages modifications across all projects

NuGet packages used across all selected projects or their dependencies that need version update in projects that reference them.

| Package Name                                   | Current Version                          | New Version | Description                                          |
|:-----------------------------------------------|:----------------------------------------:|:-----------:|:-----------------------------------------------------|
| Aspire.Hosting.AppHost                          | 9.3.0                                    | 13.0.0      | Recommended for .NET 10                              |
| Aspire.Hosting.Azure.SignalR                    | 9.4.0                                    | 13.0.0      | Recommended for .NET 10                              |
| Aspire.Hosting.PostgreSQL                       | 9.3.0                                    | 13.0.0      | Recommended for .NET 10                              |
| Aspire.Npgsql.EntityFrameworkCore.PostgreSQL    | 9.3.0;9.3.1                              | 9.5.2       | Recommended for .NET 10                              |
| Microsoft.AspNetCore.Authentication.JwtBearer   | 9.0.6                                    | 10.0.0      | Recommended for .NET 10                              |
| Microsoft.AspNetCore.Authentication.OpenIdConnect| 9.0.6;9.0.7                              | 10.0.0      | Recommended for .NET 10                              |
| Microsoft.AspNetCore.Authorization              | 9.0.7                                    | 10.0.0      | Recommended for .NET 10                              |
| Microsoft.AspNetCore.Components.Authorization   | 9.0.7                                    | 10.0.0      | Recommended for .NET 10                              |
| Microsoft.AspNetCore.OpenApi                    | 9.0.6                                    | 10.0.0      | Recommended for .NET 10                              |
| Microsoft.EntityFrameworkCore                   | 9.0.5;9.0.6                              | 10.0.0      | Recommended for .NET 10                              |
| Microsoft.EntityFrameworkCore.Tools             | 9.0.6                                    | 10.0.0      | Recommended for .NET 10                              |
| Microsoft.Extensions.Configuration.UserSecrets  | 9.0.7                                    | 10.0.0      | Recommended for .NET 10                              |
| Microsoft.Extensions.Hosting                    | 9.0.5                                    | 10.0.0      | Recommended for .NET 10                              |
| Microsoft.Extensions.Http.Resilience            | 9.4.0                                    | 10.0.0      | Recommended for .NET 10                              |
| Microsoft.Extensions.ServiceDiscovery           | 9.3.0                                    | 10.0.0      | Recommended for .NET 10                              |
| OpenTelemetry.Instrumentation.AspNetCore        | 1.12.0                                   | 1.14.0-rc.1 | Recommended for .NET 10                              |
| OpenTelemetry.Instrumentation.Http              | 1.12.0                                   | 1.14.0-rc.1 | Recommended for .NET 10                              |

### Project upgrade details
This section contains details about each project upgrade and modifications that need to be done in the project.

#### Dashy.Net.AppHost/Dashy.Net.AppHost.csproj

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

NuGet packages changes:
  - Aspire.Hosting.AppHost should be updated from `9.3.0` to `13.0.0` (recommended for .NET 10)
  - Aspire.Hosting.Azure.SignalR should be updated from `9.4.0` to `13.0.0` (recommended for .NET 10)
  - Aspire.Hosting.PostgreSQL should be updated from `9.3.0` to `13.0.0` (recommended for .NET 10)
  - Microsoft.Extensions.Configuration.UserSecrets should be updated from `9.0.7` to `10.0.0` (recommended for .NET 10)

#### Dashy.Net.ServiceDefaults/Dashy.Net.ServiceDefaults.csproj

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

NuGet packages changes:
  - Microsoft.Extensions.Http.Resilience should be updated from `9.4.0` to `10.0.0` (recommended for .NET 10)
  - Microsoft.Extensions.ServiceDiscovery should be updated from `9.3.0` to `10.0.0` (recommended for .NET 10)
  - OpenTelemetry.Instrumentation.AspNetCore should be updated from `1.12.0` to `1.14.0-rc.1` (recommended for .NET 10)
  - OpenTelemetry.Instrumentation.Http should be updated from `1.12.0` to `1.14.0-rc.1` (recommended for .NET 10)

#### Dashy.Net.ApiService/Dashy.Net.ApiService.csproj

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

NuGet packages changes:
  - Aspire.Npgsql.EntityFrameworkCore.PostgreSQL should be updated from `9.3.1` to `9.5.2` (recommended for .NET 10)
  - Microsoft.AspNetCore.Authentication.JwtBearer should be updated from `9.0.6` to `10.0.0` (recommended for .NET 10)
  - Microsoft.AspNetCore.Authentication.OpenIdConnect should be updated from `9.0.6` to `10.0.0` (recommended for .NET 10)
  - Microsoft.AspNetCore.OpenApi should be updated from `9.0.6` to `10.0.0` (recommended for .NET 10)
  - Microsoft.EntityFrameworkCore should be updated from `9.0.6` to `10.0.0` (recommended for .NET 10)
  - Microsoft.EntityFrameworkCore.Tools should be updated from `9.0.6` to `10.0.0` (recommended for .NET 10)

#### Dashy.Net.Web/Dashy.Net.Web.csproj

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

NuGet packages changes:
  - Microsoft.AspNetCore.Authentication.OpenIdConnect should be updated from `9.0.7` to `10.0.0` (recommended for .NET 10)
  - Microsoft.AspNetCore.Authorization should be updated from `9.0.7` to `10.0.0` (recommended for .NET 10)
  - Microsoft.AspNetCore.Components.Authorization should be updated from `9.0.7` to `10.0.0` (recommended for .NET 10)

#### Dashy.net.Shared/Dashy.Net.Shared.csproj

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

NuGet packages changes:
  - Microsoft.EntityFrameworkCore should be updated from `9.0.5` to `10.0.0` (recommended for .NET 10)

#### Dashy.Net.MigrationService/Dashy.Net.MigrationService.csproj

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

NuGet packages changes:
  - Aspire.Npgsql.EntityFrameworkCore.PostgreSQL should be updated from `9.3.0` to `9.5.2` (recommended for .NET 10)
  - Microsoft.Extensions.Hosting should be updated from `9.0.5` to `10.0.0` (recommended for .NET 10)

#### Dashy.Net.Tests/Dashy.Net.Tests.csproj

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

