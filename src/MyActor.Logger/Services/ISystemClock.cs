namespace MyActor.Logger.Services;

public interface ISystemClock
{
    DateTime UtcNow();
}