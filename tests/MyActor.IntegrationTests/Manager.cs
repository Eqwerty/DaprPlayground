using MyActor.IntegrationTests.Factories;
using MyActor.IntegrationTests.Redis;
using Xunit;

namespace MyActor.IntegrationTests;

public class Manager : IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        await RedisContainer.StartAsync();
        MyActorServiceFactory.InitDaprSidecar();
    }

    public async Task DisposeAsync()
    {
        await RedisContainer.DisposeAsync();
        await MyActorServiceFactory.StopDaprSidecarAsync();
    }
}