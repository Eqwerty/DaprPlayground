using Dapr.Actors.Runtime;
using MyActor.Interfaces;
using MyActor.Logger.Services;

namespace MyActor.Logger.Actors;

public class LoggerActor : Actor, ILoggerActor
{
    private const string StateName = "activity";
    private readonly ISystemClock _systemClock;

    public LoggerActor(ActorHost host, ISystemClock systemClock) : base(host)
    {
        _systemClock = systemClock;
    }

    public async Task<string> LogActivityAsync(string user)
    {
        try
        {
            await StateManager.SetStateAsync(StateName, $"Data updated at {_systemClock.UtcNow()}");
            return string.Empty;
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }
}