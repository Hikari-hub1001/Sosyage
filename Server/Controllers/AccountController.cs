using Microsoft.AspNetCore.Mvc;
using Server.Services.Account;

namespace Server.Controllers;

[ApiController]
[Route("account")]
public sealed class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpGet("registration")]
    [HttpPost("registration")]
    public IActionResult Registration([FromQuery] string name)
    {
        var id = _accountService.Register(name);
        return Ok(new { id, name });
    }

    [HttpGet("login")]
    public IActionResult Login([FromQuery] long id)
    {
        var name = _accountService.Login(id);
        if (string.IsNullOrEmpty(name))
        {
            return NotFound();
        }

        return Ok(new { id, name });
    }
}
