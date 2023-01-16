using CliWrap;
using MyActor.IntegrationTests.Redis;
using Xunit;

namespace MyActor.IntegrationTests;

public class Manager : IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        await RedisContainer.StartAsync();
        
        Cli.Wrap("dapr")
            .WithArguments(
                args => args
                    .Add("run")
                    .Add("--app-id").Add("MyActorService")
                    .Add("--dapr-http-port").Add(1501)
                    .Add("--dapr-grpc-port").Add(54201)
                    .Add("--components-path").Add("../../../../../Dapr/Components")
            )
            .ExecuteAsync();
    }

    public async Task DisposeAsync()
    {
        await RedisContainer.DisposeAsync();
        
        await Cli.Wrap("dapr")
            .WithArguments(
                args => args
                    .Add("stop").Add("MyActorService")
            ).ExecuteAsync();
    }
}