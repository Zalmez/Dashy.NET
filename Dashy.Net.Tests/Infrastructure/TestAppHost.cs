using Aspire.Hosting;

namespace Dashy.Net.Tests.Infrastructure;

public static class TestAppHost
{
    public static DistributedApplication CreateTestApp()
    {
        var builder = DistributedApplication.CreateBuilder();

        var postgres = builder.AddPostgres("testpostgres")
            .WithDataVolume("testdashydb");

        var db = postgres.AddDatabase("testdashy");

        var migrationService = builder.AddProject<Projects.Dashy_Net_MigrationService>("testmigrationservice")
            .WaitFor(db)
            .WithReference(db)
            .WithParentRelationship(db);

        var apiService = builder.AddProject<Projects.Dashy_Net_ApiService>("testapiservice")
            .WithReference(db)
            .WaitFor(db)
            .WaitFor(migrationService)
            .WithHttpHealthCheck("/health");

        builder.AddProject<Projects.Dashy_Net_Web>("testwebfrontend")
            .WithHttpHealthCheck("/health")
            .WithReference(apiService)
            .WaitFor(apiService);

        return builder.Build();
    }
}
