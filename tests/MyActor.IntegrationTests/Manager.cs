using MyActor.IntegrationTests.Factories;
using MyActor.IntegrationTests.Redis;
using Xunit;

namespace MyActor.IntegrationTests;

public class Manager : IAsyncLifetime
{
    public MyActorClientFactory MyActorClientFactory { get; set; } = new();
    public MyActorServiceFactory MyActorServiceFactory { get; set; } = new();

    public async Task InitializeAsync()
    {
        await RedisContainer.StartAsync();

        MyActorClientFactory.InitDaprSidecar();
        await Task.Delay(1000);

        MyActorServiceFactory.InitDaprSidecar();
    }

    public async Task DisposeAsync()
    {
        await RedisContainer.DisposeAsync();
        await MyActorClientFactory.StopDaprSidecarAsync();
        await MyActorServiceFactory.StopDaprSidecarAsync();
    }
}