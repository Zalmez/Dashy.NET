using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var aeskey = "5mvLd[hXf1HL=Bv1KKw#CL0+jHr!)t;L";
var ivKey = "v!rP}@;SS$7)-iVZ";

var postgres = builder.AddPostgres("postgresdb")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("dashydb")
    .WithPgAdmin();

var db = postgres.AddDatabase("dashy");

var migrationService = builder.AddProject<Projects.Dashy_Net_MigrationService>("migrationservice")
    .WaitFor(db)
    .WithReference(db)
    .WithParentRelationship(db);

var apiService = builder.AddProject<Projects.Dashy_Net_ApiService>("apiservice").WithEnvironment("DASHY_AES_KEY", aeskey)
    .WithEnvironment("DASHY_AES_IV", ivKey)
    .WithReference(db)
    .WaitFor(db)
    .WaitFor(migrationService)
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.Dashy_Net_Web>("webfrontend").WithEnvironment("DASHY_AES_KEY", aeskey)
    .WithEnvironment("DASHY_AES_IV", ivKey)
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
