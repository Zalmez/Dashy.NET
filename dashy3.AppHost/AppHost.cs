var builder = DistributedApplication.CreateBuilder(args);

var isTestRun = string.Equals(
    builder.Configuration["DASHY3_TESTING"],
    "true",
    StringComparison.OrdinalIgnoreCase);

var cache = builder.AddRedis("cache");

var postgres = builder.AddPostgres("postgres").WithContainerName("dashgres")
    .WithDataVolume("dashgres");

if (!isTestRun)
    postgres.WithPgAdmin();

var db = postgres.AddDatabase("dashy3db");

var migrations = builder.AddProject<Projects.dashy3_MigrationService>("migrations")
    .WithReference(db)
    .WaitFor(db).WithParentRelationship(db);

var apiService = builder.AddProject<Projects.dashy3_ApiService>("apiservice");
apiService = isTestRun
    ? apiService.WithHttpHealthCheck(path: "/health", endpointName: "http")
    : apiService.WithHttpHealthCheck("/health");

apiService = apiService
    .WithReference(db)
    .WaitFor(db)
    .WaitForCompletion(migrations);

var webFrontend = builder.AddProject<Projects.dashy3_Web>("webfrontend")
    .WithExternalHttpEndpoints();
webFrontend = isTestRun
    ? webFrontend.WithHttpHealthCheck(path: "/health", endpointName: "http")
    : webFrontend.WithHttpHealthCheck("/health");

webFrontend
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
