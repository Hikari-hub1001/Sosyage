using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Services.LoginBonus;

namespace Server.Controllers.Admin.LoginBonus;

[ApiController]
[Route("admin/login-bonus")]
public sealed class AdminLoginBonusController : ControllerBase
{
    private readonly ILoginBonusService _service;

    public AdminLoginBonusController(ILoginBonusService service)
    {
        _service = service;
    }

    [HttpGet("list")]
    public IActionResult List()
    {
        var items = _service.ListSummaries();
        return Ok(new { items });
    }

    [HttpPost]
    public IActionResult Register([FromBody] LoginBonusRegistration request)
    {
        try
        {
            var loginBonusId = _service.Register(request);
            return Ok(new { loginBonusId });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { error = "login bonus not found" });
        }
        catch (DbUpdateException)
        {
            return Conflict(new { error = "login bonus already exists" });
        }
    }

    [HttpGet]
    public IActionResult GetById([FromQuery] long id)
    {
        if (id <= 0)
        {
            return BadRequest(new { error = "id is required" });
        }

        var data = _service.FindById(id);
        if (data is null)
        {
            return NotFound(new { error = "login bonus not found" });
        }

        return Ok(data);
    }

    [HttpDelete]
    public IActionResult Delete([FromQuery] long id)
    {
        if (id <= 0)
        {
            return BadRequest(new { error = "id is required" });
        }

        try
        {
            var deleted = _service.DeleteById(id);
            if (!deleted)
            {
                return NotFound(new { error = "login bonus not found" });
            }

            return Ok(new { deleted = true });
        }
        catch (DbUpdateException)
        {
            return Conflict(new { error = "login bonus has related data" });
        }
    }
}
