using System.Net;
using System.Text;
using Dapr.Actors.Client;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MyActor.Client.Requests;
using MyActor.IntegrationTests.Environment;
using MyActor.Interfaces;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace MyActor.IntegrationTests;

public class MyActorTests : IClassFixture<TestsEnvironment>
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly TestsEnvironment _testsEnvironment;

    public MyActorTests(TestsEnvironment testsEnvironment, ITestOutputHelper testOutputHelper)
    {
        _testsEnvironment = testsEnvironment;
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task MyActorService_WhenThereAreNoDataStored_ShouldReturnNotFoundResponse()
    {
        //Arrange
        var httpClient = _testsEnvironment.ClientFactory.CreateClient();

        //Act
        var response = await httpClient.GetAsync("/actor?user=username");

        //Assert
        response.Should().HaveStatusCode(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task MyActorService_WhenSettingData_ShouldReturnOkResult()
    {
        //Arrange
        var httpClient = _testsEnvironment.ClientFactory.CreateClient();

        //Act
        var request = new SetDataRequest("username", "PropertyA", "PropertyB");
        var httpContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync("/actor", httpContent);

        //Assert
        response.Should().HaveStatusCode(HttpStatusCode.OK);
    }

    [Fact]
    public async Task MyActorService_WhenThereIsDataStoredForAUser_ShouldReturnDataAndOkResult()
    {
        //Arrange
        var proxyFactory = _testsEnvironment.ClientFactory.Services.GetRequiredService<IActorProxyFactory>();

        var user = "username";
        var proxy = proxyFactory.CreateActorProxy<IMyActor>(new(user), "MyActor");

        var myData = new MyData("PropertyA", "PropertyB");
        await proxy.SetDataAsync(user, myData);
        
        var httpClient = _testsEnvironment.ClientFactory.CreateClient();

        //Act
        var response = await httpClient.GetAsync($"/actor?user={user}");
        var content = await response.Content.ReadAsStringAsync();

        //Assert
        response.Should().HaveStatusCode(HttpStatusCode.OK);
        var myDataRetrieved = JsonConvert.DeserializeObject<MyData>(content);
        myDataRetrieved.Should().BeEquivalentTo(myData);
    }
}