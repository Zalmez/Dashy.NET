var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgresdb")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("dashydb")
    .WithPgAdmin();

var db = postgres.AddDatabase("dashy");

// Ensure migrationService waits for both the Postgres container and the database to be created
var migrationService = builder.AddProject<Projects.Dashy_Net_MigrationService>("migrationservice")
    .WaitFor(postgres)
    .WaitFor(db)
    .WithReference(db)
    .WithParentRelationship(db);

var apiService = builder.AddProject<Projects.Dashy_Net_ApiService>("apiservice")
    .WithReference(db)
    .WaitFor(db)
    .WaitFor(migrationService)
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.Dashy_Net_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
