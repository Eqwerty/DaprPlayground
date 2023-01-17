using System.Net;
using System.Text;
using CliWrap;
using Dapr.Client;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyActor.Client;
using MyActor.Client.Requests;
using MyActor.Interfaces;
using MyActor.Service;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace MyActor.IntegrationTests;

public class MyActorTests
{
    private const string RedisImage = "redis/redis-stack";
    private const int HostPort = 6381;
    private const int ContainerPort = 6379;

    private const string ClientAppId = "MyActorClient";
    private const int ClientAppPort = 4500;
    private const int ClientDaprHttpPort = 1400;
    private const int ClientDaprGrpcPort = 44200;
    
    private const string ServiceAppId = "MyActorService";
    private const int ServiceAppPort = 4501;
    private const int ServiceDaprHttpPort = 1401;
    private const int ServiceDaprGrpcPort = 44201;

    private const string ComponentsPath = "../../../Dapr/Components";

    private static readonly TestcontainerDatabase DbContainer = new TestcontainersBuilder<RedisTestcontainer>()
        .WithImage(RedisImage)
        .WithPortBinding(HostPort, ContainerPort)
        .Build();

    private readonly ITestOutputHelper _testOutputHelper;

    public MyActorTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task MyActor_ShouldReturnOkResult_WhenDataIsSet()
    {
        //Arrange
        await DbContainer.StartAsync();

        Cli.Wrap("dapr")
            .WithArguments(
                args => args
                    .Add("run")
                    .Add("--app-id").Add(ClientAppId)
                    .Add("--app-port").Add(ClientAppPort)
                    .Add("--dapr-http-port").Add(ClientDaprHttpPort)
                    .Add("--dapr-grpc-port").Add(ClientDaprGrpcPort)
                    .Add("--components-path").Add(ComponentsPath)
            )
            .ExecuteAsync();
        
        Cli.Wrap("dapr")
            .WithArguments(
                args => args
                    .Add("run")
                    .Add("--app-id").Add(ServiceAppId)
                    .Add("--app-port").Add(ServiceAppPort)
                    .Add("--dapr-http-port").Add(ServiceDaprHttpPort)
                    .Add("--dapr-grpc-port").Add(ServiceDaprGrpcPort)
                    .Add("--components-path").Add(ComponentsPath)
            )
            .ExecuteAsync();

        var clientFactory = new ClientFactory();
        clientFactory.CreateClient();
        var clientDaprClient = clientFactory.Services.GetRequiredService<DaprClient>();
        await clientDaprClient.WaitForSidecarAsync();

        var serviceFactory = new ServiceFactory();
        serviceFactory.CreateClient();

        var user = "user1";
        
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
        
        //Assert
        using (new AssertionScope())
        {
            getResponse1.Should().HaveStatusCode(HttpStatusCode.NotFound);
            postResponse.Should().HaveStatusCode(HttpStatusCode.OK);
            getResponse2.Should().HaveStatusCode(HttpStatusCode.OK);

            var myData = JsonConvert.DeserializeObject<MyData>(contentGetResponse2);
            myData.Should().BeEquivalentTo(expectedData);
        }

        //Cleanup
        await DbContainer.DisposeAsync();

        await Cli.Wrap("dapr")
            .WithArguments(
                args => args
                    .Add("stop").Add(ClientAppId)
            ).ExecuteAsync();
        
        await Cli.Wrap("dapr")
            .WithArguments(
                args => args
                    .Add("stop").Add(ServiceAppId)
            ).ExecuteAsync();
    }

    private class ClientFactory : WebApplicationFactory<IMyActorClientMarker>
    {
        public static readonly string HostUrl = $"http://localhost:{ClientAppPort}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseUrls(HostUrl);

            builder.UseEnvironment("Tests");

            builder.UseSetting("environmentVariables:daprHttpPort", ClientDaprHttpPort.ToString());
            builder.UseSetting("environmentVariables:daprGrpcPort", ClientDaprGrpcPort.ToString());
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
        public static readonly string HostUrl = $"http://localhost:{ServiceAppPort}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseUrls(HostUrl);

            builder.UseEnvironment("Tests");

            builder.UseSetting("environmentVariables:daprHttpPort", ServiceDaprHttpPort.ToString());
            builder.UseSetting("environmentVariables:daprGrpcPort", ServiceDaprGrpcPort.ToString());
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