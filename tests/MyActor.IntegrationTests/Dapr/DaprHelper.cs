using CliWrap;
using CliWrap.EventStream;
using Nito.AsyncEx;

namespace MyActor.IntegrationTests.Dapr;

public static class DaprHelper
{
    private const string UpAndRunningMessage = "You're up and running! Dapr logs will appear here.";
    private const int SecondsBeforeCancel = 10;

    public static async Task InitAsync(string appId, int appPort, int daprHttpPort, int daprGrpcPort, string componentsPath)
    {
        try
        {
            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(SecondsBeforeCancel));

            var countdown = new AsyncCountdownEvent(1);
            var command = Cli.Wrap("dapr")
                .WithArguments(
                    args => args
                        .Add("run")
                        .Add("--app-id").Add(appId)
                        .Add("--app-port").Add(appPort)
                        .Add("--dapr-http-port").Add(daprHttpPort)
                        .Add("--dapr-grpc-port").Add(daprGrpcPort)
                        .Add("--components-path").Add(componentsPath)
                );

            Task.Run(async () =>
            {
                await foreach (var commandEvent in command.ListenAsync(tokenSource.Token))
                {
                    switch (commandEvent)
                    {
                        case StandardOutputCommandEvent stdOut:
                            if (stdOut.Text is UpAndRunningMessage)
                            {
                                countdown.Signal();
                            }

                            break;
                    }
                }
            });

            await countdown.WaitAsync(tokenSource.Token);
        }
        catch (TaskCanceledException _)
        {
            throw new TaskCanceledException($"{appId} sidecar took more than {SecondsBeforeCancel} to be ready");
        }
    }

    public static async Task StopAsync(string appId)
    {
        await Cli.Wrap("dapr")
            .WithArguments(
                args => args
                    .Add("stop").Add(appId)
            ).ExecuteAsync();
    }
}