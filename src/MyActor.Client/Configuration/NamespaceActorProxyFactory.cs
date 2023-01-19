using Dapr.Actors;
using Dapr.Actors.Client;

namespace MyActor.Client.Configuration;

public class NamespaceActorProxyFactory : IActorProxyFactory
{
    private readonly DaprConfig _config;
    private readonly IActorProxyFactory _defaultActorProxyFactory;

    public NamespaceActorProxyFactory(ActorProxyFactory defaultActorProxyFactory, DaprConfig config)
    {
        _defaultActorProxyFactory = defaultActorProxyFactory;
        _config = config;
    }

    public TActorInterface CreateActorProxy<TActorInterface>(ActorId actorId, string actorType, ActorProxyOptions? options = null)
        where TActorInterface : IActor
    {
        var namespacedActorType = ActorNamespace.GetNamespacedActorType(_config, actorType);
        return _defaultActorProxyFactory.CreateActorProxy<TActorInterface>(actorId, namespacedActorType);
    }

    public object CreateActorProxy(ActorId actorId, Type actorInterfaceType, string actorType, ActorProxyOptions? options = null)
    {
        var namespacedActorType = ActorNamespace.GetNamespacedActorType(_config, actorType);
        return _defaultActorProxyFactory.CreateActorProxy(actorId, actorInterfaceType, namespacedActorType, options);
    }

    public ActorProxy Create(ActorId actorId, string actorType, ActorProxyOptions? options = null)
    {
        var namespacedActorType = ActorNamespace.GetNamespacedActorType(_config, actorType);
        return _defaultActorProxyFactory.Create(actorId, namespacedActorType, options);
    }
}