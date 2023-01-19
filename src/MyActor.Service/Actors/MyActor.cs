using Dapr.Actors.Runtime;
using MyActor.Interfaces;

namespace MyActor.Service.Actors;

public class MyActor : Actor, IMyActor
{
    private const string StateName = "userData";

    public MyActor(ActorHost host) : base(host)
    { }

    public async Task<string> SetDataAsync(string user, MyData data)
    {
        try
        {
            await StateManager.SetStateAsync(StateName, data);

            var actor = ProxyFactory.CreateActorProxy<ILoggerActor>(new(user), "LoggerActor");
            var errorMessage = await actor.LogActivityAsync(user);

            return errorMessage;
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }

    public async Task<(MyData?, string)> GetDataAsync(string user)
    {
        try
        {
            var data = await StateManager.TryGetStateAsync<MyData>(StateName);

            var actor = ProxyFactory.CreateActorProxy<ILoggerActor>(new(user), "LoggerActor");
            var errorMessage = await actor.LogActivityAsync(user);

            return data.HasValue ? (data.Value, errorMessage) : (null, errorMessage);
        }
        catch (Exception e)
        {
            return (null, e.Message);
        }
    }
}