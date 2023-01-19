using Dapr.Actors;
using Dapr.Actors.Client;
using Microsoft.AspNetCore.Mvc;
using MyActor.Client.Requests;
using MyActor.Interfaces;

namespace MyActor.Client.Controllers;

[ApiController]
[Route("[controller]")]
public class ActorController : ControllerBase
{
    private readonly IActorProxyFactory _proxyFactory;

    public ActorController(IActorProxyFactory proxyFactory)
    {
        _proxyFactory = proxyFactory;
    }

    [HttpGet]
    public async Task<IActionResult> GetData([FromQuery] string user)
    {
        try
        {
            var actorType = "MyActor";
            var actorId = new ActorId(user);
            var proxy = _proxyFactory.CreateActorProxy<IMyActor>(actorId, actorType);
            // var proxy = _proxyFactory.CreateActorProxy<IMyActor>(
            //     actorId,
            //     actorType,
            //     new() { HttpEndpoint = "http://localhost:1400" }
            // );

            var (myData, errorMessage) = await proxy.GetDataAsync(user);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                return BadRequest(errorMessage);
            }

            if (myData is null)
            {
                return NotFound();
            }

            return Ok(myData);
        }
        catch (Exception e)
        {
            return BadRequest($"ClientError: {e.Message}");
        }
    }

    [HttpPost]
    public async Task<IActionResult> SetData([FromBody] SetDataRequest request)
    {
        try
        {
            var actorType = "MyActor";
            var actorId = new ActorId(request.User);
            var proxy = _proxyFactory.CreateActorProxy<IMyActor>(actorId, actorType);
            // var proxy = _proxyFactory.CreateActorProxy<IMyActor>(
            //     actorId,
            //     actorType,
            //     new() { HttpEndpoint = "http://localhost:1400" }
            // );

            var myData = new MyData(request.PropertyA, request.PropertyB);
            var errorMessage = await proxy.SetDataAsync(request.User, myData);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                return BadRequest(errorMessage);
            }

            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest($"ClientError: {e.Message}");
        }
    }
}