using Dapr.Actors;

namespace MyActor.Interfaces;

public interface IMyActor : IActor
{
    Task SetDataAsync(MyData data);

    Task<MyData?> GetDataAsync();
}