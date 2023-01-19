namespace MyActor.Client.Configuration;

public static class ActorNamespace
{
    public static string GetNamespacedActorType(DaprConfig config, string actorType)
    {
        return !string.IsNullOrWhiteSpace(config.Namespace) ? $"{actorType}.{config.Namespace}" : actorType;
    }
}