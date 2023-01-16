using Microsoft.AspNetCore.Mvc;

namespace MyActor.Client.Controllers;

[ApiController]
[Route("[controller]")]
public class PingController : ControllerBase
{
    [HttpGet]
    public IActionResult Ping()
    {
        return Ok("Pong");
    }
}