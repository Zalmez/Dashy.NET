var builder = DistributedApplication.CreateBuilder(args);
var compose = builder.AddDockerComposeEnvironment("compose");
var isTestRun = string.Equals(
    builder.Configuration["DASHY3_TESTING"],
    "true",
    StringComparison.OrdinalIgnoreCase);

var cache = builder.AddRedis("cache");

#pragma warning disable ASPIRECOMPUTE003
var registry = builder.AddContainerRegistry(
    "<name>",
    "<endpoint>",
    "<repository>"
);

var postgres = builder.AddPostgres("postgres").WithContainerName("dashgres")
    .WithDataVolume("dashgres");

if (!isTestRun)
    postgres.WithPgAdmin();

var db = postgres.AddDatabase("dashy3db");

var migrations = builder.AddProject<Projects.dashy3_MigrationService>("migrations")
    .WithReference(db)
    .WaitFor(db).WithParentRelationship(db)
    .PublishAsDockerComposeService((resource, service) => { service.Name = "migrations"; })
    .WithContainerRegistry(registry);

var apiService = builder.AddProject<Projects.dashy3_ApiService>("apiservice")
    .PublishAsDockerComposeService((resource, service) => { service.Name = "apiservice"; })
    .WithContainerRegistry(registry);
apiService = isTestRun
    ? apiService.WithHttpHealthCheck(path: "/health", endpointName: "http")
    : apiService.WithHttpHealthCheck("/health");

apiService = apiService
    .WithReference(db)
    .WaitFor(db)
    .WaitForCompletion(migrations);

var webFrontend = builder.AddProject<Projects.dashy3_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .PublishAsDockerComposeService((resource, service) => { service.Name = "webfrontend"; })
    .WithContainerRegistry(registry);
webFrontend = isTestRun
    ? webFrontend.WithHttpHealthCheck(path: "/health", endpointName: "http")
    : webFrontend.WithHttpHealthCheck("/health");

webFrontend
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
