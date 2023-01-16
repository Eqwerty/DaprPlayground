using CliWrap;
using Xunit;

namespace MyActor.IntegrationTests;

public class MyActorTests
{
    [Fact]
    public async Task Test()
    {
        Cli.Wrap("dapr")
            .WithArguments(
                args => args
                    .Add("run")
                    .Add("--app-id").Add("MyActorService")
                    .Add("--dapr-http-port").Add(1501)
                    .Add("--dapr-grpc-port").Add(54201)
                    .Add("--components-path").Add("../../../../../dapr/components")
            )
            .ExecuteAsync();

        await Task.Delay(30000);
        
        await Cli.Wrap("dapr")
            .WithArguments(
                args => args
                    .Add("stop").Add("MyActorService")
            ).ExecuteAsync();
    }
}