using Microsoft.AspNetCore.Mvc;
using Server.Services.AdminAccount;

namespace Server.Controllers.Admin.Account;

[ApiController]
[Route("admin/account")]
public sealed class AdminAccountController : ControllerBase
{
    private readonly IAdminAccountService _adminAccountService;

    public AdminAccountController(IAdminAccountService adminAccountService)
    {
        _adminAccountService = adminAccountService;
    }

    [HttpGet]
    public IActionResult Get([FromQuery] int offset = 0, [FromQuery] int limit = 50)
    {
        if (offset < 0)
        {
            offset = 0;
        }

        if (limit < 1)
        {
            limit = 1;
        }
        else if (limit > 200)
        {
            limit = 200;
        }

        var result = _adminAccountService.GetAccounts(offset, limit);
        return Ok(result);
    }

    [HttpDelete("{id:long}")]
    public IActionResult Delete(long id)
    {
        if (!_adminAccountService.DeleteAccount(id))
        {
            return NotFound();
        }

        return NoContent();
    }
}
