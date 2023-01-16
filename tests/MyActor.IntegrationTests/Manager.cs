using MyActor.IntegrationTests.Factories;
using MyActor.IntegrationTests.Redis;
using Xunit;

namespace MyActor.IntegrationTests;

public class Manager : IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        await RedisContainer.StartAsync();

        MyActorClientFactory.InitDaprSidecar();
        await Task.Delay(1000);

        MyActorServiceFactory.InitDaprSidecar();

        var clientFactory = new MyActorClientFactory();
        clientFactory.CreateClient();

        var serviceFactory = new MyActorServiceFactory();
        serviceFactory.CreateClient();
        
        await Task.Delay(3000);
    }

    public async Task DisposeAsync()
    {
        await RedisContainer.DisposeAsync();
        await MyActorClientFactory.StopDaprSidecarAsync();
        await MyActorServiceFactory.StopDaprSidecarAsync();
    }
}