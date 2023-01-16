using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using MyActor.Client;
using MyActor.IntegrationTests.Dapr;

namespace MyActor.IntegrationTests.Factories;

public class MyActorClientFactory : WebApplicationFactory<IMyActorClientMarker>
{
    private readonly string _hostUrl = "http://localhost:4500";

    public static void InitDaprSidecar()
    {
        DaprHelper.Init(
            "MyActorClient",
            4500,
            1500,
            54200,
            "../../../Dapr/Components"
        );
    }

    public static async Task StopDaprSidecarAsync()
    {
        await DaprHelper.Stop("MyActorClient");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseUrls(_hostUrl);
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