using CliWrap;
using CliWrap.EventStream;
using MyActor.IntegrationTests.Environment;
using Nito.AsyncEx;

namespace MyActor.IntegrationTests.Dapr;

public class DaprInitializer
{
    private const string UpAndRunningMessage = "You're up and running! Dapr logs will appear here.";
    private const int SecondsBeforeCancel = 10;

    private readonly Settings _settings;

    public DaprInitializer(Settings settings)
    {
        _settings = settings;
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
                        .Add("--app-id").Add(_settings.AppId)
                        .Add("--app-port").Add(_settings.AppPort)
                        .Add("--dapr-http-port").Add(_settings.DaprHttpPort)
                        .Add("--dapr-grpc-port").Add(_settings.DaprGrpcPort)
                        .Add("--components-path").Add(Settings.ComponentsPath)
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
            throw new TaskCanceledException($"{_settings.AppId} sidecar took more than {SecondsBeforeCancel} to be ready");
        }
    }

    public async Task StopAsync()
    {
        await Cli.Wrap("dapr")
            .WithArguments(
                args => args
                    .Add("stop").Add(_settings.AppId)
            ).ExecuteAsync();
    }
}