using System.Net;
using System.Text;
using CliWrap;
using Dapr.Client;
using FluentAssertions;
using FluentAssertions.Execution;
using Google.Protobuf;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using MyActor.Client;
using MyActor.Client.Requests;
using MyActor.IntegrationTests.Dapr;
using MyActor.IntegrationTests.Factories;
using MyActor.IntegrationTests.Redis;
using MyActor.Interfaces;
using MyActor.Logger;
using MyActor.Logger.Services;
using MyActor.Service;
using Newtonsoft.Json;
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

            await DaprHelper.InitAsync(
                Settings.Logger.AppId,
                Settings.Logger.AppPort,
                Settings.Logger.DaprHttpPort,
                Settings.Logger.DaprGrpcPort,
                Settings.Logger.ComponentsPath
            );

            await DaprHelper.InitAsync(
                Settings.Service.AppId,
                Settings.Service.AppPort,
                Settings.Service.DaprHttpPort,
                Settings.Service.DaprGrpcPort,
                Settings.Service.ComponentsPath
            );

            await DaprHelper.InitAsync(
                Settings.Client.AppId,
                Settings.Client.AppPort,
                Settings.Client.DaprHttpPort,
                Settings.Client.DaprGrpcPort,
                Settings.Client.ComponentsPath
            );

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

            await DaprHelper.StopAsync(Settings.Logger.AppId);
            await DaprHelper.StopAsync(Settings.Service.AppId);
            await DaprHelper.StopAsync(Settings.Client.AppId);
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