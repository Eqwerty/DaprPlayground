using MyActor.IntegrationTests.Dapr;

namespace MyActor.IntegrationTests.Factories;

public class MyActorServiceFactory
{
    public static void InitDaprSidecar()
    {
        DaprHelper.Init(
            "MyActorService",
            1501,
            54201,
            "../../../../../Dapr/Components"
        );
    }

    public static async Task StopDaprSidecarAsync()
    {
        await DaprHelper.Stop("MyActorService");
    }
}