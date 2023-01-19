using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyActor.Service;

namespace MyActor.IntegrationTests.Environment;

public class ServiceFactory : WebApplicationFactory<IMyActorServiceMarker>
{
    public static readonly string HostUrl = $"http://localhost:{Settings.Service.AppPort}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseUrls(HostUrl);

        builder.UseEnvironment("Tests");

        builder.UseSetting("environmentVariables:daprHttpPort", Settings.Service.DaprHttpPort.ToString());
        builder.UseSetting("environmentVariables:daprGrpcPort", Settings.Service.DaprGrpcPort.ToString());

        builder.ConfigureServices(services => services.AddActors(options => options.HttpEndpoint = $"http://localhost:{Settings.Service.DaprHttpPort}"));
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