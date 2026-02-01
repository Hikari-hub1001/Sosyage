using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers;

[ApiController]
public sealed class RootController : ControllerBase
{
    [HttpGet("/")]
    public IActionResult Get()
    {
        return Content("Sosyage server: index", "text/plain; charset=utf-8");
    }
}
