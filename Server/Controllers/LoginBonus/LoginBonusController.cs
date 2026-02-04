using Microsoft.AspNetCore.Mvc;
using Server.Services.LoginBonus;

namespace Server.Controllers.LoginBonus;

[ApiController]
[Route("login-bonus")]
public sealed class LoginBonusController : ControllerBase
{
    private readonly ILoginBonusService _loginBonusService;

    public LoginBonusController(ILoginBonusService loginBonusService)
    {
        _loginBonusService = loginBonusService;
    }

    [HttpPost("claim")]
    public IActionResult Claim([FromBody] LoginBonusClaimRequest request)
    {
        if (request.id <= 0)
        {
            return NotFound(new { error = "Not found accountId" });
        }

        var response = _loginBonusService.ClaimForAccount(request.id);
        if (response is null)
        {
            return NotFound(new { error = "login bonus not found" });
        }

        return Ok(response);
    }
}
