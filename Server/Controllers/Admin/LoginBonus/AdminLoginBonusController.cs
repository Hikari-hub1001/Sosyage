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

    [HttpPost]
    public IActionResult Register([FromBody] LoginBonusRegistration request)
    {
        try
        {
            var monthId = _service.Register(request);
            return Ok(new { monthId });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (DbUpdateException)
        {
            return Conflict(new { error = "month already exists" });
        }
    }

    [HttpGet]
    public IActionResult GetByMonth([FromQuery] string month)
    {
        if (string.IsNullOrWhiteSpace(month))
        {
            return BadRequest(new { error = "month is required" });
        }

        var data = _service.FindByMonth(month);
        if (data is null)
        {
            return NotFound(new { error = "month not found" });
        }

        return Ok(data);
    }

    [HttpDelete]
    public IActionResult Delete([FromQuery] string month)
    {
        if (string.IsNullOrWhiteSpace(month))
        {
            return BadRequest(new { error = "month is required" });
        }

        try
        {
            var deleted = _service.DeleteByMonth(month);
            if (!deleted)
            {
                return NotFound(new { error = "month not found" });
            }

            return Ok(new { deleted = true });
        }
        catch (DbUpdateException)
        {
            return Conflict(new { error = "month has related data" });
        }
    }
}
