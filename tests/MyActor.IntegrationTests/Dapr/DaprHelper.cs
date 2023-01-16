using CliWrap;

namespace MyActor.IntegrationTests.Dapr;

public static class DaprHelper
{
    public static Task Init(string appId, int appPort, int daprHttpPort, int daprGrpcPort, string componentsPath)
    {
        return Cli.Wrap("dapr")
            .WithArguments(
                args => args
                    .Add("run")
                    .Add("--app-id").Add(appId)
                    .Add("--app-port").Add(appPort)
                    .Add("--dapr-http-port").Add(daprHttpPort)
                    .Add("--dapr-grpc-port").Add(daprGrpcPort)
                    .Add("--components-path").Add(componentsPath)
            )
            .ExecuteAsync();
    }

    public static Task Stop(string appId)
    {
        return Cli.Wrap("dapr")
            .WithArguments(
                args => args
                    .Add("stop").Add(appId)
            ).ExecuteAsync();
    }
}