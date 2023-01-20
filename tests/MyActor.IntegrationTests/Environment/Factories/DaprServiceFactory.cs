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
    private readonly Settings _settings;

    protected DaprServiceFactory(Settings settings)
    {
        _settings = settings;
        _daprHttpEndpoint = $"http://localhost:{_settings.DaprHttpPort}";
    }

    public string HostUrl => $"http://localhost:{_settings.AppPort}";

    public async Task InitializeSidecarAsync()
    {
        await DaprHelper.InitAsync(_settings);

        var client = Services.GetRequiredService<DaprClient>();
        await client.WaitForSidecarAsync();
    }

    public async Task StopSidecarAsync()
    {
        await DaprHelper.StopAsync(_settings);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseUrls(HostUrl);

        builder.UseEnvironment("Tests");

        builder.UseSetting("environmentVariables:daprHttpPort", Settings.Client.DaprHttpPort.ToString());
        builder.UseSetting("environmentVariables:daprGrpcPort", Settings.Client.DaprGrpcPort.ToString());

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