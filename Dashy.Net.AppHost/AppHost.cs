using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var config = builder.Configuration;
var authAuthority = config["auth_authority"] ?? "";
var authClientId = config["auth_clientid"] ?? "";
var authClientSecret = config["auth_clientsecret"] ?? "";

var postgres = builder.AddPostgres("postgresdb")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("dashydb");

var db = postgres.AddDatabase("dashy");


var apiService = builder.AddProject<Projects.Dashy_Net_ApiService>("apiservice")
    .WithReference(postgres)
    .WithReference(db)
    .WaitFor(db)
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.Dashy_Net_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithEnvironment("auth_authority", authAuthority).WithEnvironment("auth_clientid", authClientId).WithEnvironment("auth_clientsecret", authClientSecret)
    .WithReference(apiService)
    .WaitFor(apiService);

postgres.WithPgAdmin();

builder.Build().Run();
