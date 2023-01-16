using MyActor.IntegrationTests.Factories;
using MyActor.IntegrationTests.Redis;
using Xunit;
using Xunit.Abstractions;

namespace MyActor.IntegrationTests;

public class MyActorTests //: IClassFixture<Manager>
{
    private readonly ITestOutputHelper _testOutputHelper;

    public MyActorTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task Test()
    {
        await RedisContainer.StartAsync();
        MyActorServiceFactory.InitDaprSidecar();
        
        await Task.Delay(30000);
        
        await RedisContainer.DisposeAsync();
        await MyActorServiceFactory.StopDaprSidecarAsync();
    }
}