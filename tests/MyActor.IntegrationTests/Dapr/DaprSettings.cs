namespace MyActor.IntegrationTests.Dapr;

public class DaprSettings
{
    public const string ComponentsPath = "../../../Dapr/Components";

    public string AppId { get; private init; } = string.Empty;
    public int AppPort { get; private init; }
    public int DaprHttpPort { get; private init; }
    public int DaprGrpcPort { get; private init; }

    public static DaprSettings Client => new()
    {
        AppId = "MyActorClient-tests",
        AppPort = 4500,
        DaprHttpPort = 1400,
        DaprGrpcPort = 44200
    };

    public static DaprSettings Service => new()
    {
        AppId = "MyActorService-tests",
        AppPort = 4501,
        DaprHttpPort = 1401,
        DaprGrpcPort = 44201
    };

    public static DaprSettings Logger => new()
    {
        AppId = "MyActorLogger-tests",
        AppPort = 4502,
        DaprHttpPort = 1402,
        DaprGrpcPort = 44202
    };
}