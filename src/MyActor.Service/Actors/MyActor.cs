using Dapr.Actors.Runtime;
using MyActor.Interfaces;

namespace MyActor.Service.Actors;

public class MyActor : Actor, IMyActor
{
    public MyActor(ActorHost host) : base(host)
    { }
    
    private const string StateName = "userTopics";

    public async Task SetDataAsync(MyData data)
    {
        await StateManager.SetStateAsync(StateName, data);
    }

    public async Task<MyData?> GetDataAsync()
    {
        var data = await StateManager.TryGetStateAsync<MyData>(StateName);

        return data.HasValue ? data.Value : null;
    }
}