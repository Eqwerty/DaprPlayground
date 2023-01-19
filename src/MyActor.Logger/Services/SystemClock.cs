namespace MyActor.Logger.Services;

public class SystemClock : ISystemClock
{
    public DateTime UtcNow()
    {
        return DateTime.UtcNow;
    }
}