namespace MyActor.IntegrationTests.Environment;

public class Settings
{
    public string AppId { get; private init; } = string.Empty;
    public int AppPort { get; private init; }
    public int DaprHttpPort { get; private init; }
    public int DaprGrpcPort { get; private init; }
    public string ComponentsPath { get; private init; } = string.Empty;

    public static Settings Client => new()
    {
        AppId = "MyActorClient-tests",
        AppPort = 4500,
        DaprHttpPort = 1400,
        DaprGrpcPort = 44200,
        ComponentsPath = "../../../Dapr/Components"
    };

    public static Settings Service => new()
    {
        AppId = "MyActorService-tests",
        AppPort = 4501,
        DaprHttpPort = 1401,
        DaprGrpcPort = 44201,
        ComponentsPath = "../../../Dapr/Components"
    };

    public static Settings Logger => new()
    {
        AppId = "MyActorLogger-tests",
        AppPort = 4502,
        DaprHttpPort = 1402,
        DaprGrpcPort = 44202,
        ComponentsPath = "../../../Dapr/Components"
    };
}