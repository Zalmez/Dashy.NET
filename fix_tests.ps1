# PowerShell script to fix all test compilation errors

Write-Host "Fixing test compilation errors..." -ForegroundColor Green

# Fix Dashy.Net.Tests project file - remove ContainerLifetime reference that doesn't exist
$testProjectPath = "g:\dashy.net\Dashy.Net.Tests\Dashy.Net.Tests.csproj"
Write-Host "Updating test project file..." -ForegroundColor Yellow

$projectContent = @'
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="coverlet.collector" Version="6.0.2">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="Aspire.Hosting.Testing" Version="9.0.0" />
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="9.0.0" />
    <PackageReference Include="Moq" Version="4.20.72" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="9.0.0" />
    <PackageReference Include="Testcontainers.PostgreSql" Version="4.0.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Dashy.Net.ApiService\Dashy.Net.ApiService.csproj" />
    <ProjectReference Include="..\Dashy.Net.AppHost\Dashy.Net.AppHost.csproj" />
    <ProjectReference Include="..\Dashy.net.Shared\Dashy.Net.Shared.csproj" />
    <ProjectReference Include="..\Dashy.Net.Web\Dashy.Net.Web.csproj" />
  </ItemGroup>

</Project>
'@

Set-Content -Path $testProjectPath -Value $projectContent -Encoding UTF8

Write-Host "Project file updated successfully!" -ForegroundColor Green
Write-Host "Test project compilation errors should now be resolved." -ForegroundColor Green
Write-Host ""
Write-Host "Running build to verify..." -ForegroundColor Yellow
