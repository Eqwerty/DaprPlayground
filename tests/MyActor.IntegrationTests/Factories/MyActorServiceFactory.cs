using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using MyActor.IntegrationTests.Dapr;
using MyActor.Service;

namespace MyActor.IntegrationTests.Factories;

public class MyActorServiceFactory : WebApplicationFactory<IMyActorServiceMarker>
{
    private readonly string _hostUrl = "http://localhost:4501";

    public static void InitDaprSidecar()
    {
        DaprHelper.Init(
            "MyActorService",
            4501,
            1501,
            54201,
            "../../../Dapr/Components"
        );
    }

    public static async Task StopDaprSidecarAsync()
    {
        await DaprHelper.Stop("MyActorService");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseUrls(_hostUrl);

        builder.UseEnvironment("Tests");

        builder.UseSetting("environmentVariables:daprHttpPort", "1501");
        builder.UseSetting("environmentVariables:daprGrpcPort", "54201");
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