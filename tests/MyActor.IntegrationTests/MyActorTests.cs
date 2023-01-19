using System.Net;
using System.Text;
using CliWrap;
using CliWrap.EventStream;
using Dapr.Client;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using MyActor.Client;
using MyActor.Client.Requests;
using MyActor.IntegrationTests.Factories;
using MyActor.IntegrationTests.Redis;
using MyActor.Interfaces;
using MyActor.Logger;
using MyActor.Logger.Services;
using MyActor.Service;
using Newtonsoft.Json;
using Nito.AsyncEx;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace MyActor.IntegrationTests;

public class MyActorTests
{
    private static readonly DateTime UtcNow = DateTime.UtcNow;

    private readonly ITestOutputHelper _testOutputHelper;

    public MyActorTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task MyActor_ShouldReturnOkResult_WhenDataIsSet()
    {
        try
        {
            //Arrange
            await RedisContainer.StartAsync();

            var loggerCountdown = new AsyncCountdownEvent(1);
            var loggerCommand = Cli.Wrap("dapr")
                .WithArguments(
                    args => args
                        .Add("run")
                        .Add("--app-id").Add(Settings.Logger.AppId)
                        .Add("--app-port").Add(Settings.Logger.AppPort)
                        .Add("--dapr-http-port").Add(Settings.Logger.DaprHttpPort)
                        .Add("--dapr-grpc-port").Add(Settings.Logger.DaprGrpcPort)
                        .Add("--components-path").Add(Settings.Logger.ComponentsPath)
                );

            Task.Run(async () =>
            {
                await foreach (var commandEvent in loggerCommand.ListenAsync())
                {
                    switch (commandEvent)
                    {
                        case StandardOutputCommandEvent stdOut:
                            if (stdOut.Text is "You're up and running! Dapr logs will appear here.")
                            {
                                loggerCountdown.Signal();
                            }

                            break;
                    }
                }
            });

            await loggerCountdown.WaitAsync();

            var serviceCountdown = new AsyncCountdownEvent(1);
            var serviceCommand = Cli.Wrap("dapr")
                .WithArguments(
                    args => args
                        .Add("run")
                        .Add("--app-id").Add(Settings.Service.AppId)
                        .Add("--app-port").Add(Settings.Service.AppPort)
                        .Add("--dapr-http-port").Add(Settings.Service.DaprHttpPort)
                        .Add("--dapr-grpc-port").Add(Settings.Service.DaprGrpcPort)
                        .Add("--components-path").Add(Settings.Service.ComponentsPath)
                );

            Task.Run(async () =>
            {
                await foreach (var commandEvent in serviceCommand.ListenAsync())
                {
                    switch (commandEvent)
                    {
                        case StandardOutputCommandEvent stdOut:
                            if (stdOut.Text is "You're up and running! Dapr logs will appear here.")
                            {
                                serviceCountdown.Signal();
                            }

                            break;
                    }
                }
            });

            await serviceCountdown.WaitAsync();

            var clientCountdown = new AsyncCountdownEvent(1);
            var clientCommand = Cli.Wrap("dapr")
                .WithArguments(
                    args => args
                        .Add("run")
                        .Add("--app-id").Add(Settings.Client.AppId)
                        .Add("--app-port").Add(Settings.Client.AppPort)
                        .Add("--dapr-http-port").Add(Settings.Client.DaprHttpPort)
                        .Add("--dapr-grpc-port").Add(Settings.Client.DaprGrpcPort)
                        .Add("--components-path").Add(Settings.Client.ComponentsPath)
                );

            Task.Run(async () =>
            {
                await foreach (var commandEvent in clientCommand.ListenAsync())
                {
                    switch (commandEvent)
                    {
                        case StandardOutputCommandEvent stdOut:
                            if (stdOut.Text is "You're up and running! Dapr logs will appear here.")
                            {
                                clientCountdown.Signal();
                            }

                            break;
                    }
                }
            });

            await clientCountdown.WaitAsync();

            var clientFactory = new ClientFactory();
            clientFactory.CreateClient();
            var clientDaprClient = clientFactory.Services.GetRequiredService<DaprClient>();
            await clientDaprClient.WaitForSidecarAsync();

            var serviceFactory = new ServiceFactory();
            serviceFactory.CreateClient();
            var serviceDaprClient = serviceFactory.Services.GetRequiredService<DaprClient>();
            await serviceDaprClient.WaitForSidecarAsync();

            var loggerFactory = new LoggerFactory();
            loggerFactory.CreateClient();
            var loggerDaprClient = loggerFactory.Services.GetRequiredService<DaprClient>();
            await loggerDaprClient.WaitForSidecarAsync();

            var user = "user1";

            await Task.Delay(4_000);

            //Act
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new(ClientFactory.HostUrl);

            var getResponse1 = await httpClient.GetAsync($"/actor?user={user}");
            var contentGetResponse1 = await getResponse1.Content.ReadAsStringAsync();
            _testOutputHelper.WriteLine($"{nameof(contentGetResponse1)}: {contentGetResponse1}");

            _testOutputHelper.WriteLine("");

            var expectedData = new MyData("property1", "property2");
            var request = new SetDataRequest(user, expectedData.PropertyA, expectedData.PropertyB);
            var httpContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            var postResponse = await httpClient.PostAsync("/actor", httpContent);
            var postResponseContent = await postResponse.Content.ReadAsStringAsync();
            _testOutputHelper.WriteLine($"postResponseContent: {postResponseContent}");

            _testOutputHelper.WriteLine("");

            var getResponse2 = await httpClient.GetAsync($"/actor?user={user}");
            var contentGetResponse2 = await getResponse2.Content.ReadAsStringAsync();
            _testOutputHelper.WriteLine($"{nameof(contentGetResponse2)}: {contentGetResponse2}");

            var loggerHttpClient = new HttpClient();
            loggerHttpClient.BaseAddress = new($"http://localhost:{Settings.Logger.DaprHttpPort}");
            loggerHttpClient.DefaultRequestHeaders.Add("dapr-app-id", Settings.Logger.AppId);

            var loggerResponse = await loggerHttpClient.GetAsync("/v1.0/actors/LoggerActor/user1/state/activity");
            var log = await loggerResponse.Content.ReadAsStringAsync();
            _testOutputHelper.WriteLine("");
            _testOutputHelper.WriteLine($"log: {log}");

            //Assert
            using (new AssertionScope())
            {
                getResponse1.Should().HaveStatusCode(HttpStatusCode.NotFound);
                postResponse.Should().HaveStatusCode(HttpStatusCode.OK);
                getResponse2.Should().HaveStatusCode(HttpStatusCode.OK);

                var myData = JsonConvert.DeserializeObject<MyData>(contentGetResponse2);
                myData.Should().BeEquivalentTo(expectedData);

                log.Should().Be($"\"Data updated at {UtcNow}\"");
            }
        }
        finally
        {
            //Cleanup
            await RedisContainer.DisposeAsync();

            await Cli.Wrap("dapr")
                .WithArguments(
                    args => args
                        .Add("stop").Add(Settings.Client.AppId)
                ).ExecuteAsync();

            await Cli.Wrap("dapr")
                .WithArguments(
                    args => args
                        .Add("stop").Add(Settings.Service.AppId)
                ).ExecuteAsync();

            await Cli.Wrap("dapr")
                .WithArguments(
                    args => args
                        .Add("stop").Add(Settings.Logger.AppId)
                ).ExecuteAsync();
        }
    }

    private class ClientFactory : WebApplicationFactory<IMyActorClientMarker>
    {
        public static readonly string HostUrl = $"http://localhost:{Settings.Client.AppPort}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseUrls(HostUrl);

            builder.UseEnvironment("Tests");

            builder.UseSetting("environmentVariables:daprHttpPort", Settings.Client.DaprHttpPort.ToString());
            builder.UseSetting("environmentVariables:daprGrpcPort", Settings.Client.DaprGrpcPort.ToString());

            builder.ConfigureServices(services => services.AddActors(options => options.HttpEndpoint = $"http://localhost:{Settings.Client.DaprHttpPort}"));
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

    private class ServiceFactory : WebApplicationFactory<IMyActorServiceMarker>
    {
        public static readonly string HostUrl = $"http://localhost:{Settings.Service.AppPort}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseUrls(HostUrl);

            builder.UseEnvironment("Tests");

            builder.UseSetting("environmentVariables:daprHttpPort", Settings.Service.DaprHttpPort.ToString());
            builder.UseSetting("environmentVariables:daprGrpcPort", Settings.Service.DaprGrpcPort.ToString());

            builder.ConfigureServices(services => services.AddActors(options => options.HttpEndpoint = $"http://localhost:{Settings.Service.DaprHttpPort}"));
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

    private class LoggerFactory : WebApplicationFactory<IMyActorLoggerMarker>
    {
        public static readonly string HostUrl = $"http://localhost:{Settings.Logger.AppPort}";

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
                systemClock.UtcNow().Returns(UtcNow);

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
}