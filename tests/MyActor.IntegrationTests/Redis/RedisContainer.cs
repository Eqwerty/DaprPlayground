using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace MyActor.IntegrationTests.Redis;

public static class RedisContainer
{
    private const string RedisImage = "redis/redis-stack";
    private const int HostPort = 6381;
    private const int ContainerPort = 6379;

    private static readonly TestcontainerDatabase DbContainer = new TestcontainersBuilder<RedisTestcontainer>()
        .WithImage(RedisImage)
        .WithPortBinding(HostPort, ContainerPort)
        .Build();

    public static async Task StartAsync()
    {
        await DbContainer.StartAsync();
    }

    public static async Task DisposeAsync()
    {
        await DbContainer.DisposeAsync();
    }
}