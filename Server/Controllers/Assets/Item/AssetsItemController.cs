using Microsoft.AspNetCore.Mvc;
using Server.Services.Assets.Item;

namespace Server.Controllers.Assets.Item;

[ApiController]
[Route("assets/item")]
public sealed class AssetsItemController : ControllerBase
{
    private readonly IAssetsItemService _assetItemService;

    public AssetsItemController(IAssetsItemService assetItemService)
    {
        _assetItemService = assetItemService;
    }

    [HttpGet("list")]
    public IActionResult List()
    {
        var items = _assetItemService.ListItems();
        return Ok(new { items });
    }
}
