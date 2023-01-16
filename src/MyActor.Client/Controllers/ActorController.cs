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
    [HttpGet]
    public async Task<IActionResult> GetData([FromQuery] string user)
    {
        var actorType = "MyActor";
        var actorId = new ActorId(user);
        var proxy = ActorProxy.Create<IMyActor>(actorId, actorType);

        try
        {
            var myData = await proxy.GetDataAsync();

            if (myData is null)
            {
                return NotFound();
            }

            return Ok(myData);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> SetData([FromBody] SetDataRequest request)
    {
        var actorType = "MyActor";
        var actorId = new ActorId(request.User);
        var proxy = ActorProxy.Create<IMyActor>(actorId, actorType);

        var myData = new MyData(request.PropertyA, request.PropertyB);
        await proxy.SetDataAsync(myData);

        return Ok();
    }
}