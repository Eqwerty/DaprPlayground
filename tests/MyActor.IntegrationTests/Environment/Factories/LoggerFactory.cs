using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MyActor.Logger;
using MyActor.Logger.Services;
using NSubstitute;

namespace MyActor.IntegrationTests.Environment.Factories;

public class LoggerFactory : DaprServiceFactory<IMyActorLoggerMarker>
{
    public static readonly DateTime UtcNow = DateTime.UtcNow;

    public LoggerFactory() : base(Settings.Logger)
    { }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureServices(services =>
        {
            var systemClock = Substitute.For<ISystemClock>();
            systemClock.UtcNow().ReturnsForAnyArgs(UtcNow);

            services.RemoveAll<ISystemClock>();
            services.AddSingleton(systemClock);
        });
    }
}