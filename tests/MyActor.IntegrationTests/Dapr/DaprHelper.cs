using CliWrap;
using CliWrap.EventStream;
using MyActor.IntegrationTests.Environment;
using Nito.AsyncEx;

namespace MyActor.IntegrationTests.Dapr;

public static class DaprHelper
{
    private const string UpAndRunningMessage = "You're up and running! Dapr logs will appear here.";
    private const int SecondsBeforeCancel = 10;

    public static async Task InitAsync(Settings settings)
    {
        try
        {
            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(SecondsBeforeCancel));

            var countdown = new AsyncCountdownEvent(1);
            var command = Cli.Wrap("dapr")
                .WithArguments(
                    args => args
                        .Add("run")
                        .Add("--app-id").Add(settings.AppId)
                        .Add("--app-port").Add(settings.AppPort)
                        .Add("--dapr-http-port").Add(settings.DaprHttpPort)
                        .Add("--dapr-grpc-port").Add(settings.DaprGrpcPort)
                        .Add("--components-path").Add(settings.ComponentsPath)
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
            throw new TaskCanceledException($"{settings.AppId} sidecar took more than {SecondsBeforeCancel} to be ready");
        }
    }

    public static async Task StopAsync(Settings settings)
    {
        await Cli.Wrap("dapr")
            .WithArguments(
                args => args
                    .Add("stop").Add(settings.AppId)
            ).ExecuteAsync();
    }
}