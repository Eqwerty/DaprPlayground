namespace MyActor.IntegrationTests.Environment;

public static class Settings
{
    public static class Client
    {
        public const string AppId = "MyActorClient-tests";
        public const int AppPort = 4500;
        public const int DaprHttpPort = 1400;
        public const int DaprGrpcPort = 44200;
        public const string ComponentsPath = "../../../Dapr/Components";
    }

    public static class Service
    {
        public const string AppId = "MyActorService-tests";
        public const int AppPort = 4501;
        public const int DaprHttpPort = 1401;
        public const int DaprGrpcPort = 44201;
        public const string ComponentsPath = "../../../Dapr/Components";
    }

    public static class Logger
    {
        public const string AppId = "MyActorLogger-tests";
        public const int AppPort = 4502;
        public const int DaprHttpPort = 1402;
        public const int DaprGrpcPort = 44202;
        public const string ComponentsPath = "../../../Dapr/Components";
    }
}