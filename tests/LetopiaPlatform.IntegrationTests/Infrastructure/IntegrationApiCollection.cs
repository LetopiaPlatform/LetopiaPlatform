using Xunit;

namespace LetopiaPlatform.IntegrationTests.Infrastructure;

/// <summary>
/// Single shared API test host for all integration tests in this collection
/// to avoid parallel host startup races.
/// </summary>
[CollectionDefinition(Name)]
public class IntegrationApiCollection : ICollectionFixture<AgentIntegrationWebApplicationFactory>
{
    public const string Name = "Letopia API integration";
}
