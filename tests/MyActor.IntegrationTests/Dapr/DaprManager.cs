using CliWrap;
using CliWrap.EventStream;
using Nito.AsyncEx;

namespace MyActor.IntegrationTests.Dapr;

public class DaprManager
{
    private const string UpAndRunningMessage = "You're up and running! Dapr logs will appear here.";
    private const int SecondsBeforeCancel = 10;

    private readonly DaprSettings _daprSettings;

    public DaprManager(DaprSettings daprSettings)
    {
        _daprSettings = daprSettings;
    }

    public async Task InitAsync()
    {
        try
        {
            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(SecondsBeforeCancel));

            var countdown = new AsyncCountdownEvent(1);
            var command = Cli.Wrap("dapr")
                .WithArguments(
                    args => args
                        .Add("run")
                        .Add("--app-id").Add(_daprSettings.AppId)
                        .Add("--app-port").Add(_daprSettings.AppPort)
                        .Add("--dapr-http-port").Add(_daprSettings.DaprHttpPort)
                        .Add("--dapr-grpc-port").Add(_daprSettings.DaprGrpcPort)
                        .Add("--components-path").Add(DaprSettings.ComponentsPath)
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
        catch (TaskCanceledException)
        {
            throw new TaskCanceledException($"{_daprSettings.AppId} sidecar took more than {SecondsBeforeCancel} to be ready");
        }
    }

    public async Task StopAsync()
    {
        await Cli.Wrap("dapr")
            .WithArguments(
                args => args
                    .Add("stop").Add(_daprSettings.AppId)
            ).ExecuteAsync();
    }
}