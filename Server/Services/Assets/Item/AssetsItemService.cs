using Microsoft.EntityFrameworkCore;
using Server.Data;

namespace Server.Services.Assets.Item;

public sealed class AssetsItemService : IAssetsItemService
{
    private readonly AppDbContext _db;

    public AssetsItemService(AppDbContext db)
    {
        _db = db;
    }

    public IReadOnlyList<AssetsItemDto> ListItems()
    {
        return _db.ItemMasters
            .AsNoTracking()
            .OrderBy(item => item.Id)
            .Select(item => new AssetsItemDto(item.Id, item.Name))
            .ToList();
    }
}
