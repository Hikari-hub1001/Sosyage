using Microsoft.AspNetCore.Mvc;
using Server.Services.Account;

namespace Server.Controllers.Account;

[ApiController]
[Route("account")]
public sealed class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpPost("registration")]
    public IActionResult Registration([FromBody] RegistrationRequest request)
    {
        var id = _accountService.Register(request.Name);
        return Ok(new { id, name = request.Name });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var name = _accountService.Login(request.Id);
        if (string.IsNullOrEmpty(name))
        {
            return NotFound();
        }

        return Ok(new { id = request.Id, name });
    }

    public sealed record RegistrationRequest(string Name);
    public sealed record LoginRequest(long Id);
}
