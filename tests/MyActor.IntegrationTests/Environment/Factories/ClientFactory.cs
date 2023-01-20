using MyActor.Client;
using MyActor.IntegrationTests.Dapr;

namespace MyActor.IntegrationTests.Environment.Factories;

public class ClientFactory : DaprServiceFactory<IMyActorClientMarker>
{
    public ClientFactory() : base(DaprSettings.Client)
    { }
}