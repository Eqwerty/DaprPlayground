using Dapr.Client;
using Microsoft.Extensions.DependencyInjection;
using MyActor.IntegrationTests.Dapr;
using MyActor.IntegrationTests.Redis;
using Xunit;

namespace MyActor.IntegrationTests.Environment;

public class IntegrationTestsEnvironment : IAsyncLifetime
{
    public static readonly DateTime UtcNow = DateTime.UtcNow;
    public LoggerFactory LoggerFactory { get; private set; }
    public ClientFactory ClientFactory { get; private set; }
    public ServiceFactory ServiceFactory { get; private set; }

    public async Task InitializeAsync()
    {
        await RedisContainer.StartAsync();

        await DaprHelper.InitAsync(
            Settings.Logger.AppId,
            Settings.Logger.AppPort,
            Settings.Logger.DaprHttpPort,
            Settings.Logger.DaprGrpcPort,
            Settings.Logger.ComponentsPath
        );

        await DaprHelper.InitAsync(
            Settings.Service.AppId,
            Settings.Service.AppPort,
            Settings.Service.DaprHttpPort,
            Settings.Service.DaprGrpcPort,
            Settings.Service.ComponentsPath
        );

        await DaprHelper.InitAsync(
            Settings.Client.AppId,
            Settings.Client.AppPort,
            Settings.Client.DaprHttpPort,
            Settings.Client.DaprGrpcPort,
            Settings.Client.ComponentsPath
        );

        ClientFactory = new();
        ClientFactory.CreateClient();
        var clientDaprClient = ClientFactory.Services.GetRequiredService<DaprClient>();
        await clientDaprClient.WaitForSidecarAsync();

        ServiceFactory = new();
        ServiceFactory.CreateClient();
        var serviceDaprClient = ServiceFactory.Services.GetRequiredService<DaprClient>();
        await serviceDaprClient.WaitForSidecarAsync();

        LoggerFactory = new(UtcNow);
        LoggerFactory.CreateClient();
        var loggerDaprClient = LoggerFactory.Services.GetRequiredService<DaprClient>();
        await loggerDaprClient.WaitForSidecarAsync();
    }

    public async Task DisposeAsync()
    {
        await RedisContainer.DisposeAsync();

        await DaprHelper.StopAsync(Settings.Logger.AppId);
        await DaprHelper.StopAsync(Settings.Service.AppId);
        await DaprHelper.StopAsync(Settings.Client.AppId);
    }
}