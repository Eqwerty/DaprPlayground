using System.Net;
using System.Text;
using FluentAssertions;
using FluentAssertions.Execution;
using MyActor.Client.Requests;
using MyActor.IntegrationTests.Environment;
using MyActor.Interfaces;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace MyActor.IntegrationTests;

public class MyActorTests : IClassFixture<IntegrationTestsEnvironment>
{
    private readonly ITestOutputHelper _testOutputHelper;

    public MyActorTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task MyActor_ShouldReturnOkResult_WhenDataIsSet()
    {
        //Arrange
        var user = "user1";
        await Task.Delay(4_000);

        var httpClient = new HttpClient();
        httpClient.BaseAddress = new(ClientFactory.HostUrl);

        //Act
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

            log.Should().Be($"\"Data updated at {IntegrationTestsEnvironment.UtcNow}\"");
        }
    }
}