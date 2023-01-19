using Dapr.Actors;

namespace MyActor.Interfaces;

public interface IMyActor : IActor
{
    Task<string> SetDataAsync(string user, MyData data);

    Task<(MyData?, string)> GetDataAsync(string user);
}