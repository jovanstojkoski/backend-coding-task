using NUnit.Framework;

namespace Claims.IntegrationTests.Api;

[NonParallelizable]
public abstract class ApiIntegrationTestBase
{
    [SetUp]
    public async Task ResetDatabase()
    {
        await IntegrationTestFixture.ResetDatabaseAsync();
    }
}
