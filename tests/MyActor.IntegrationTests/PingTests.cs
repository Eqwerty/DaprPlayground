using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using MyActor.Client;
using Xunit;
using Xunit.Abstractions;

namespace MyActor.IntegrationTests;

public class PingTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public PingTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task Test()
    {
        var factory = new WebApplicationFactoryFixture();

        var client = factory.CreateClient();

        var response = await client.GetAsync("/Ping");

        var content = await response.Content.ReadAsStringAsync();

        response.Should().BeSuccessful();
        content.Should().Be("Pong");
    }

    [Fact]
    public async Task Wait()
    {
        var factory = new WebApplicationFactoryFixture();

        factory.CreateClient();

        await Task.Delay(10000);

        await factory.DisposeAsync();
    }

    public class WebApplicationFactoryFixture : WebApplicationFactory<IMyActorClientMarker>
    {
        private readonly string _hostUrl = "https://localhost:6000"; // we can use any free port

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseUrls(_hostUrl);
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