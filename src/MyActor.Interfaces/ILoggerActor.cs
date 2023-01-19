using Dapr.Actors;

namespace MyActor.Interfaces;

public interface ILoggerActor : IActor
{
    Task<string> LogActivityAsync();
}