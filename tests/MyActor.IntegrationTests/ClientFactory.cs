using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyActor.Client;
using MyActor.IntegrationTests.Environment;

namespace MyActor.IntegrationTests;

public class ClientFactory : WebApplicationFactory<IMyActorClientMarker>
{
    public static readonly string HostUrl = $"http://localhost:{Settings.Client.AppPort}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseUrls(HostUrl);

        builder.UseEnvironment("Tests");

        builder.UseSetting("environmentVariables:daprHttpPort", Settings.Client.DaprHttpPort.ToString());
        builder.UseSetting("environmentVariables:daprGrpcPort", Settings.Client.DaprGrpcPort.ToString());

        builder.ConfigureServices(services => services.AddActors(options => options.HttpEndpoint = $"http://localhost:{Settings.Client.DaprHttpPort}"));
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