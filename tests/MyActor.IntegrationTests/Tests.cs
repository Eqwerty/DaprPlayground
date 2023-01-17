using System.Text;
using FluentAssertions;
using FluentAssertions.Execution;
using MyActor.Client.Requests;
using MyActor.IntegrationTests.Factories;
using MyActor.Interfaces;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace MyActor.IntegrationTests;

public class Tests : IClassFixture<Manager>
{
    private readonly ITestOutputHelper _testOutputHelper;

    public Tests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task Test()
    {
        //Arrange
        var client = new HttpClient();
        client.BaseAddress = new(MyActorClientFactory.HostUrl);

        var dataRequest = new SetDataRequest("user1", "prop1", "prop2");
        var json = JsonConvert.SerializeObject(dataRequest);
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

        //Act
        var postResponse = await client.PostAsync("/actor", httpContent);
        var getResponse = await client.GetAsync("/actor?user=user1");

        //Assert
        using (new AssertionScope())
        {
            postResponse.Should().BeSuccessful();
            getResponse.Should().BeSuccessful();

            var postResponseContent = await postResponse.Content.ReadAsStringAsync();
            _testOutputHelper.WriteLine($"Post response content: {postResponseContent}");

            var getResponseContent = await getResponse.Content.ReadAsStringAsync();
            _testOutputHelper.WriteLine($"Get response content: {getResponseContent}");

            JsonConvert.DeserializeObject<MyData>(getResponseContent).Should().Be(new MyData(dataRequest.PropertyA, dataRequest.PropertyB));
        }
    }
}