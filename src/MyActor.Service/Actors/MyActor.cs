using Dapr.Actors.Runtime;
using MyActor.Interfaces;

namespace MyActor.Service.Actors;

public class MyActor : Actor, IMyActor
{
    public MyActor(ActorHost host) : base(host)
    { }

    public async Task SetDataAsync(MyData data)
    {
        await StateManager.SetStateAsync(Id.GetId(), data);
    }

    public async Task<MyData?> GetDataAsync()
    {
        var data = await StateManager.TryGetStateAsync<MyData>(Id.GetId());

        return data.HasValue ? data.Value : null;
    }
}