﻿using System.Net;
using System.Net.Http.Headers;
using System.Text;
using FluentAssertions;
using MyActor.Client.Requests;
using MyActor.IntegrationTests.Factories;
using MyActor.IntegrationTests.Redis;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace MyActor.IntegrationTests;

public class MyActorTests //: IClassFixture<Manager>
{
    private readonly Manager _manager;
    private readonly ITestOutputHelper _testOutputHelper;

    public MyActorTests(ITestOutputHelper testOutputHelper)
    {
        // _manager = manager;
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task Test()
    {
        //Init
        await RedisContainer.StartAsync();

        MyActorClientFactory.InitDaprSidecar();
        await Task.Delay(1000);

        MyActorServiceFactory.InitDaprSidecar();

        var clientFactory = new MyActorClientFactory();
        clientFactory.CreateClient();

        var serviceFactory = new MyActorServiceFactory();
        serviceFactory.CreateClient();

        //Test
        var client = new HttpClient();
        client.BaseAddress = new("http://localhost:4500");

        var dataRequest = new SetDataRequest("user1", "once", "diez");
        
        var json = JsonConvert.SerializeObject(dataRequest);
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await client.PostAsync("/actor", httpContent);

        response.Should().BeSuccessful();
        
        response = await client.GetAsync("/actor?user=user1");
        response.Should().BeSuccessful();
        var content = await response.Content.ReadAsStringAsync();
        _testOutputHelper.WriteLine(content);

        //Dispose
        await RedisContainer.DisposeAsync();
        await MyActorClientFactory.StopDaprSidecarAsync();
        await MyActorServiceFactory.StopDaprSidecarAsync();
    }
}