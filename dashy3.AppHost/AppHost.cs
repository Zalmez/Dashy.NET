var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var postgres = builder.AddPostgres("postgres").WithContainerName("dashgres")
    .WithDataVolume("dashgres")
    .WithPgAdmin();

var db = postgres.AddDatabase("dashy3db");

var migrations = builder.AddProject<Projects.dashy3_MigrationService>("migrations")
    .WithReference(db)
    .WaitFor(db).WithParentRelationship(db);

var apiService = builder.AddProject<Projects.dashy3_ApiService>("apiservice")
    .WithHttpHealthCheck("/health").WithReference(db).WaitFor(db)
    .WaitForCompletion(migrations);

builder.AddProject<Projects.dashy3_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
