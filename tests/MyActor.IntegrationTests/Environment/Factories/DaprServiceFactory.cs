using Dapr.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyActor.IntegrationTests.Dapr;

namespace MyActor.IntegrationTests.Environment.Factories;

public abstract class DaprServiceFactory<TServiceMarker> : WebApplicationFactory<TServiceMarker> where TServiceMarker : class
{
    private readonly string _daprHttpEndpoint;
    private readonly DaprManager _daprManager;
    private readonly DaprSettings _daprSettings;

    protected DaprServiceFactory(DaprSettings daprSettings)
    {
        _daprSettings = daprSettings;
        _daprManager = new(_daprSettings);
        _daprHttpEndpoint = $"http://localhost:{_daprSettings.DaprHttpPort}";
    }

    private string HostUrl => $"http://localhost:{_daprSettings.AppPort}";

    public async Task InitializeSidecarAsync()
    {
        await _daprManager.InitAsync();

        var client = Services.GetRequiredService<DaprClient>();
        await client.WaitForSidecarAsync();
    }

    public async Task StopSidecarAsync()
    {
        await _daprManager.StopAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseUrls(HostUrl);

        builder.UseEnvironment("Tests");

        builder.UseSetting("environmentVariables:daprHttpPort", _daprSettings.DaprHttpPort.ToString());
        builder.UseSetting("environmentVariables:daprGrpcPort", _daprSettings.DaprGrpcPort.ToString());

        builder.ConfigureServices(services => services.AddActors(options => options.HttpEndpoint = _daprHttpEndpoint));
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var dummyHost = builder.Build();

        builder.ConfigureWebHost(webHostBuilder => webHostBuilder.UseKestrel());

        var host = builder.Build();
        host.Start();

        return dummyHost;
    }
}