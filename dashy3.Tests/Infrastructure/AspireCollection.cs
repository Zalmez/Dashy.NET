namespace dashy3.Tests.Infrastructure;

/// <summary>
/// Defines the "Aspire" xUnit collection.
/// All test classes decorated with [Collection("Aspire")] share ONE AspireFixture instance,
/// meaning the AppHost starts only once for the entire test run.
/// </summary>
[CollectionDefinition("Aspire")]
public class AspireCollection : ICollectionFixture<AspireFixture>
{
    // This class is intentionally empty.
    // ICollectionFixture<AspireFixture> wires up the shared fixture.
}
