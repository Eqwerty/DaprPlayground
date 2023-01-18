using CliWrap;
using CliWrap.EventStream;
using Nito.AsyncEx;

var tokenSource = new CancellationTokenSource();

Console.WriteLine("Initializing...");

await DaprManager.RunDaprAsync(tokenSource.Token);

Console.WriteLine("Waiting...");

for (var i = 0; i < 30; i++)
{
    await Task.Delay(1000);
    Console.WriteLine(i + 1);
}

tokenSource.Cancel();

public static class DaprManager
{
    public static async Task RunDaprAsync(CancellationToken cancellationToken)
    {
        var countdownEvent = new AsyncCountdownEvent(1);
        
        var cmd = Cli.Wrap("dapr")
            .WithArguments(
                args => args
                    .Add("run")
                    .Add("--app-id").Add("AppId")
                    .Add("--app-port").Add("1234")
                    .Add("--dapr-http-port").Add("5678")
                    .Add("--dapr-grpc-port").Add("9101")
                    .Add("--app-health-check-path").Add("/healthz")
            );

        Task.Run(async () =>
        {
            await foreach (var cmdEvent in cmd.ListenAsync(cancellationToken))
            {
                switch (cmdEvent)
                {
                    case StandardOutputCommandEvent stdOut:
                        Console.WriteLine($"Out> {stdOut.Text}");
                        if (stdOut.Text is "You're up and running! Dapr logs will appear here.")
                        {
                            countdownEvent.Signal();
                        }
                        break;
                }
            }
        }, cancellationToken);
        
        await countdownEvent.WaitAsync(cancellationToken);
    }
}