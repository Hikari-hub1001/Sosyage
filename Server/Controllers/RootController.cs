using Microsoft.AspNetCore.Mvc;
using Server.Services.Root;

namespace Server.Controllers;

[ApiController]
public sealed class RootController : ControllerBase
{
    private readonly IRootService _rootService;

    public RootController(IRootService rootService)
    {
        _rootService = rootService;
    }

    [HttpGet("/")]
    public IActionResult Get()
    {
        var message = _rootService.GetIndexMessage();
        return Content(message, "text/plain; charset=utf-8");
    }
}
