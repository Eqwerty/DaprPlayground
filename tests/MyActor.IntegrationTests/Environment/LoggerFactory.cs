using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using MyActor.Logger;
using MyActor.Logger.Services;
using NSubstitute;

namespace MyActor.IntegrationTests.Environment;

public class LoggerFactory : WebApplicationFactory<IMyActorLoggerMarker>
{
    private static readonly string HostUrl = $"http://localhost:{Settings.Logger.AppPort}";
    public static readonly DateTime UtcNow = DateTime.UtcNow;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseUrls(HostUrl);

        builder.UseEnvironment("Tests");

        builder.UseSetting("environmentVariables:daprHttpPort", Settings.Logger.DaprHttpPort.ToString());
        builder.UseSetting("environmentVariables:daprGrpcPort", Settings.Logger.DaprGrpcPort.ToString());

        builder.ConfigureServices(services =>
        {
            services.AddActors(options => options.HttpEndpoint = $"http://localhost:{Settings.Logger.DaprHttpPort}");

            var systemClock = Substitute.For<ISystemClock>();
            systemClock.UtcNow().ReturnsForAnyArgs(UtcNow);

            services.RemoveAll<ISystemClock>();
            services.AddSingleton(systemClock);
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var dummyHost = builder.Build();

        builder.ConfigureWebHost(webHostBuilder => webHostBuilder.UseKestrel());

        var host = builder.Build();
        host.Start();

        return dummyHost;
    }
}