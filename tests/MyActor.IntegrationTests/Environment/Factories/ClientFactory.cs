using MyActor.Client;

namespace MyActor.IntegrationTests.Environment.Factories;

public class ClientFactory : DaprServiceFactory<IMyActorClientMarker>
{
    public ClientFactory() : base(Settings.Client)
    { }
}