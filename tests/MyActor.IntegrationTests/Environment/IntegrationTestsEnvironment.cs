using MyActor.IntegrationTests.Environment.Factories;
using MyActor.IntegrationTests.Redis;
using Xunit;

namespace MyActor.IntegrationTests.Environment;

public class IntegrationTestsEnvironment : IAsyncLifetime
{
    public ClientFactory ClientFactory { get; } = new();
    public ServiceFactory ServiceFactory { get; } = new();
    public LoggerFactory LoggerFactory { get; } = new();

    public async Task InitializeAsync()
    {
        await RedisContainer.StartAsync();

        await ClientFactory.InitializeSidecarAsync();
        await ServiceFactory.InitializeSidecarAsync();
        await LoggerFactory.InitializeSidecarAsync();

        await Task.Delay(3_000);
    }

    public async Task DisposeAsync()
    {
        await RedisContainer.DisposeAsync();

        await ClientFactory.StopSidecarAsync();
        await ServiceFactory.StopSidecarAsync();
        await LoggerFactory.StopSidecarAsync();
    }
}